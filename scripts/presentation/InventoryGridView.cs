using Godot;
using System;
using System.Collections.Generic;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Presentation;

public partial class InventoryGridView : Control
{
	[Signal]
	public delegate void CellPressedEventHandler(Vector2I cell, int buttonIndex);

	[Signal]
	public delegate void CellReleasedEventHandler(Vector2I cell, int buttonIndex);

	[Signal]
	public delegate void CellDoubleClickedEventHandler(Vector2I cell, int buttonIndex);

	[Signal]
	public delegate void CellHoveredEventHandler(Vector2I cell);

	[Signal]
	public delegate void PointerExitedEventHandler();

	[Export(PropertyHint.Range, "16,96,1")]
	public int CellSize { get; set; } = 42;

	[Export]
	public Color GridLineColor { get; set; } = new(0.30f, 0.30f, 0.34f, 1.0f);

	[Export]
	public Color EmptyCellColor { get; set; } = new(0.12f, 0.12f, 0.14f, 1.0f);

	[Export]
	public Color ItemFillColor { get; set; } = new(0.73f, 0.66f, 0.47f, 0.90f);

	[Export]
	public Color SelectedOutlineColor { get; set; } = new(1.0f, 0.93f, 0.54f, 1.0f);

	[Export]
	public Color PreviewValidColor { get; set; } = new(0.42f, 0.87f, 0.55f, 0.45f);

	[Export]
	public Color PreviewInvalidColor { get; set; } = new(0.92f, 0.32f, 0.28f, 0.45f);

	[Export]
	public Color MarkFillColor { get; set; } = new(0.18f, 0.72f, 0.38f, 0.95f);

	[Export]
	public Color MarkTextColor { get; set; } = new(0.96f, 1.0f, 0.96f, 1.0f);

	[Export]
	public Color HiddenItemFillColor { get; set; } = new(0.22f, 0.24f, 0.27f, 0.96f);

	[Export]
	public Color HiddenItemBorderColor { get; set; } = new(0.44f, 0.47f, 0.52f, 0.95f);

	[Export]
	public Color SearchSpinnerColor { get; set; } = new(0.92f, 0.95f, 0.98f, 0.95f);

	private GridContainerComponent? _container;
	private Vector2I _selectedCell = Vector2I.Zero;
	private ContainerItemRecord? _selectedRecord;
	private Rect2I? _previewRect;
	private bool _previewValid = true;
	private bool _hasFocusVisual;
	private HashSet<string> _markedInstanceIds = [];

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
		MouseExited += OnMouseExited;
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (_container?.SearchRuntime?.HasActiveSearch == true)
		{
			QueueRedraw();
		}
	}

	public void SetContainer(GridContainerComponent? container)
	{
		if (_container == container)
		{
			return;
		}

		if (_container != null)
		{
			_container.ContainerChanged -= OnContainerChanged;
		}

		_container = container;
		if (_container != null)
		{
			_container.ContainerChanged += OnContainerChanged;
		}

		RefreshMinimumSize();
		QueueRedraw();
	}

	public void SetSelection(Vector2I selectedCell, ContainerItemRecord? selectedRecord, bool hasFocusVisual)
	{
		_selectedCell = selectedCell;
		_selectedRecord = selectedRecord;
		_hasFocusVisual = hasFocusVisual;
		QueueRedraw();
	}

	public void SetPreview(Rect2I? previewRect, bool previewValid)
	{
		_previewRect = previewRect;
		_previewValid = previewValid;
		QueueRedraw();
	}

	public void SetMarkedItems(IEnumerable<string>? instanceIds)
	{
		_markedInstanceIds = instanceIds == null ? [] : new HashSet<string>(instanceIds);
		QueueRedraw();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			if (TryGetCell(motion.Position, out Vector2I hoverCell))
			{
				EmitSignal(SignalName.CellHovered, hoverCell);
			}

			return;
		}

		if (@event is not InputEventMouseButton mouseButton || !TryGetCell(mouseButton.Position, out Vector2I cell))
		{
			return;
		}

		if (mouseButton.Pressed)
		{
			if (mouseButton.DoubleClick)
			{
				EmitSignal(SignalName.CellDoubleClicked, cell, (int)mouseButton.ButtonIndex);
				return;
			}

			EmitSignal(SignalName.CellPressed, cell, (int)mouseButton.ButtonIndex);
			return;
		}

		EmitSignal(SignalName.CellReleased, cell, (int)mouseButton.ButtonIndex);
	}

	public override void _Draw()
	{
		Vector2I gridSize = _container?.GetGridSize() ?? Vector2I.Zero;
		if (gridSize == Vector2I.Zero)
		{
			return;
		}

		DrawGridBackground(gridSize);
		DrawItems();
		DrawPreview();
		DrawSelection();
	}

	private void DrawGridBackground(Vector2I gridSize)
	{
		for (int y = 0; y < gridSize.Y; y++)
		{
			for (int x = 0; x < gridSize.X; x++)
			{
				Rect2 rect = GetCellRect(new Vector2I(x, y));
				DrawRect(rect, EmptyCellColor, true);
				DrawRect(rect, GridLineColor, false, 1.0f);
			}
		}
	}

	private void DrawItems()
	{
		if (_container == null)
		{
			return;
		}

		foreach (ContainerItemRecord record in _container.ItemRecords)
		{
			Rect2 rect = GetRect(record.GetOccupiedRect());
			ContainerSearchVisualState searchVisualState = _container.SearchRuntime?.GetVisualState(record.Item.InstanceId)
				?? new ContainerSearchVisualState(false, true, false);
			if (searchVisualState.UsesSearchRules && !searchVisualState.IsRevealed)
			{
				DrawRect(rect, HiddenItemFillColor, true);
				DrawRect(rect, HiddenItemBorderColor, false, 2.0f);
				if (searchVisualState.IsSearching)
				{
					DrawSearchSpinner(rect);
				}

				if (_markedInstanceIds.Contains(record.Item.InstanceId))
				{
					DrawMarkedIndicator(rect);
				}
				continue;
			}

			Color fillColor = ItemValueClassifier.GetFillColor(record.Item.Definition);
			Color borderColor = ItemValueClassifier.GetBorderColor(record.Item.Definition);
			Color textColor = ItemValueClassifier.GetTextColor(record.Item.Definition);
			DrawRect(rect, fillColor, true);
			DrawRect(rect, borderColor, false, 2.0f);

			string glyph = ResolveGlyph(record).ToString();
			Vector2 textPos = rect.Position + new Vector2(6.0f, 18.0f);
			DrawString(ThemeDB.FallbackFont, textPos, glyph, HorizontalAlignment.Left, -1.0f, 16, textColor);

			string countText = record.Item.HasLimitedUses
				? $"{record.Item.RemainingUses}"
				: $"x{record.Item.StackCount}";
			Vector2 countPos = rect.End - new Vector2(25.0f, 6.0f);
			DrawString(ThemeDB.FallbackFont, countPos, countText, HorizontalAlignment.Right, -1.0f, 12, textColor);

			if (_markedInstanceIds.Contains(record.Item.InstanceId))
			{
				DrawMarkedIndicator(rect);
			}
		}
	}

	private void DrawPreview()
	{
		if (!_previewRect.HasValue)
		{
			return;
		}

		Rect2 preview = GetRect(_previewRect.Value);
		DrawRect(preview, _previewValid ? PreviewValidColor : PreviewInvalidColor, true);
		DrawRect(preview, _previewValid ? PreviewValidColor.Darkened(0.35f) : PreviewInvalidColor.Darkened(0.25f), false, 2.0f);
	}

	private void DrawSelection()
	{
		if (_container == null || _container.GetGridSize() == Vector2I.Zero || !_hasFocusVisual)
		{
			return;
		}

		Rect2 selectionRect = _selectedRecord != null
			? GetRect(_selectedRecord.GetOccupiedRect())
			: GetCellRect(_selectedCell);
		DrawRect(selectionRect.Grow(1.0f), SelectedOutlineColor, false, 3.0f);
	}

	private void DrawMarkedIndicator(Rect2 rect)
	{
		Rect2 markRect = new(
			rect.End.X - 18.0f,
			rect.End.Y - 18.0f,
			14.0f,
			14.0f);
		DrawRect(markRect, MarkFillColor, true);
		DrawRect(markRect, MarkTextColor, false, 1.0f);
		DrawString(
			ThemeDB.FallbackFont,
			new Vector2(markRect.Position.X + 2.0f, markRect.Position.Y + 11.0f),
			"✓",
			HorizontalAlignment.Left,
			-1.0f,
			12,
			MarkTextColor);
	}

	private void DrawSearchSpinner(Rect2 rect)
	{
		Vector2 center = rect.GetCenter();
		float radius = Mathf.Min(rect.Size.X, rect.Size.Y) * 0.24f;
		float rotation = ((Time.GetTicksMsec() % 1000L) / 1000.0f) * Mathf.Tau;
		for (int segment = 0; segment < 8; segment++)
		{
			float angle = rotation + (segment * Mathf.Tau / 8.0f);
			float alpha = 0.28f + (segment / 7.0f) * 0.64f;
			Color segmentColor = new(SearchSpinnerColor.R, SearchSpinnerColor.G, SearchSpinnerColor.B, alpha);
			Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
			DrawLine(center + direction * (radius * 0.45f), center + direction * radius, segmentColor, 2.0f);
		}
	}

	private Rect2 GetCellRect(Vector2I cell)
	{
		return new Rect2(cell.X * CellSize, cell.Y * CellSize, CellSize, CellSize);
	}

	private Rect2 GetRect(Rect2I rect)
	{
		return new Rect2(
			rect.Position.X * CellSize,
			rect.Position.Y * CellSize,
			rect.Size.X * CellSize,
			rect.Size.Y * CellSize);
	}

	private bool TryGetCell(Vector2 position, out Vector2I cell)
	{
		cell = Vector2I.Zero;
		Vector2I gridSize = _container?.GetGridSize() ?? Vector2I.Zero;
		if (gridSize == Vector2I.Zero)
		{
			return false;
		}

		int x = Mathf.FloorToInt(position.X / CellSize);
		int y = Mathf.FloorToInt(position.Y / CellSize);
		if (x < 0 || y < 0 || x >= gridSize.X || y >= gridSize.Y)
		{
			return false;
		}

		cell = new Vector2I(x, y);
		return true;
	}

	public bool TryGetCellFromGlobalPosition(Vector2 globalPosition, out Vector2I cell)
	{
		Transform2D inverseTransform = GetGlobalTransformWithCanvas().AffineInverse();
		Vector2 localPosition = inverseTransform * globalPosition;
		return TryGetCell(localPosition, out cell);
	}

	private void RefreshMinimumSize()
	{
		Vector2I gridSize = _container?.GetGridSize() ?? Vector2I.Zero;
		CustomMinimumSize = new Vector2(gridSize.X * CellSize, gridSize.Y * CellSize);
		Size = CustomMinimumSize;
	}

	private void OnContainerChanged()
	{
		RefreshMinimumSize();
		QueueRedraw();
	}

	private void OnMouseExited()
	{
		EmitSignal(SignalName.PointerExited);
	}

	private static char ResolveGlyph(ContainerItemRecord record)
	{
		foreach (char character in record.Item.Definition.DisplayName)
		{
			if (char.IsLetterOrDigit(character))
			{
				return char.ToUpperInvariant(character);
			}
		}

		return 'X';
	}
}
