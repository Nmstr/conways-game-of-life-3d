using Godot;
using System.Collections.Generic;
using Godot.Collections;


public partial class Grid : GridMap
{
	private int _emptyCellIdx = -1;
	private int _livingCellIdx = 0;
	private int _ghostCellIdx = 1;
	
	public override void _Ready()
	{
		GetNode<Timer>("../TickTimer").Timeout += Tick;
	}

	public void Tick()
	{
		CleanGhostCells();
		
		// Calculate changed cells
		System.Collections.Generic.Dictionary<Vector3I, int> cellChanges = new System.Collections.Generic.Dictionary<Vector3I, int>();
		foreach (Vector3I cell in GetUsedCells())
		{
			int cellUpdate = GetCellUpdate(cell);
			if (GetCellItem(cell) != cellUpdate)
			{
				cellChanges[cell] = cellUpdate;
			}
		}

		// Apply changes
		foreach (Vector3I cell in cellChanges.Keys)
		{
			SetCellItem(cell, cellChanges[cell]);
		}
		
		GD.Print("Tick finished");
	}

	// Removes all ghost cells that do not neighbor a living cell
	// Also replaces all empty cells around living cells with ghost cells
	public void CleanGhostCells()
	{
		Array<Vector3I> usedCells = GetUsedCells();
		foreach (var cell in usedCells)
		{
			if (GetCellItem(cell) == _ghostCellIdx)
			{
				// Ghost Cell
				bool hasLivingNeighbor = false;
				foreach (Vector3I neighbor in GetNeighborCells(cell))
				{
					if (GetCellItem(neighbor) == _livingCellIdx)
					{
						hasLivingNeighbor = true;
						break;
					}
				}
				if (!hasLivingNeighbor)
				{
					SetCellItem(cell, _emptyCellIdx);
				}
			}
			else
			{
				// Living Cell
				foreach (Vector3I neighbor in GetNeighborCells(cell))
				{
					if (GetCellItem(neighbor) == _emptyCellIdx)
					{
						SetCellItem(neighbor, _ghostCellIdx);
					}
				}
			}
		}
	}

	public int GetCellUpdate(Vector3I cell)
	{
		int livingNeighborCount = 0;
		foreach (Vector3I neighbor in GetNeighborCells(cell))
		{
			if (GetCellItem(neighbor) == _livingCellIdx)
			{
				livingNeighborCount++;
			}
		}

		if (GetCellItem(cell) == _livingCellIdx)
		{
			if (livingNeighborCount < 2 || livingNeighborCount > 3) 
			{
				return _ghostCellIdx;
			}
			return _livingCellIdx;
		}
		if (livingNeighborCount == 3)
		{
			return _livingCellIdx;
		}
		return _ghostCellIdx;
	}

	public List<Vector3I> GetNeighborCells(Vector3I cell)
	{
		List<Vector3I> neighbors = new List<Vector3I>();
		for (int x = -1; x < 2; x++)
		{
			for (int y = -1; y < 2; y++)
			{
				for (int z = -1; z < 2; z++)
				{
					if (x == 0 && y == 0 && z == 0)
					{
						continue;
					}
					neighbors.Add(cell + new Vector3I(x, y, z));
				}
			}
		}
		return neighbors;
	}
}
