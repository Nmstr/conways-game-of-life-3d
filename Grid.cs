using Godot;
using System.Collections.Generic;
using Godot.Collections;


public partial class Grid : GridMap
{
	private const int EmptyCellIdx = -1;
	private const int LivingCellIdx = 0;
	private const int GhostCellIdx = 1;
	private int _iterationCount = 0;
	
	public override void _Ready()
	{
		GetNode<Timer>("../TickTimer").Timeout += RunIteration;
	}

	private void RunIteration()
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

		_iterationCount++;
		GD.Print("Finished Iteration: ", _iterationCount);
	}

	// Removes all ghost cells that do not neighbor a living cell
	// Also replaces all empty cells around living cells with ghost cells
	private void CleanGhostCells()
	{
		Array<Vector3I> usedCells = GetUsedCells();
		foreach (var cell in usedCells)
		{
			if (GetCellItem(cell) == GhostCellIdx)
			{
				// Ghost Cell
				bool hasLivingNeighbor = false;
				foreach (Vector3I neighbor in GetNeighborCells(cell))
				{
					if (GetCellItem(neighbor) == LivingCellIdx)
					{
						hasLivingNeighbor = true;
						break;
					}
				}
				if (!hasLivingNeighbor)
				{
					SetCellItem(cell, EmptyCellIdx);
				}
			}
			else
			{
				// Living Cell
				foreach (Vector3I neighbor in GetNeighborCells(cell))
				{
					if (GetCellItem(neighbor) == EmptyCellIdx)
					{
						SetCellItem(neighbor, GhostCellIdx);
					}
				}
			}
		}
	}

	private int GetCellUpdate(Vector3I cell)
	{
		int livingNeighborCount = 0;
		foreach (Vector3I neighbor in GetNeighborCells(cell))
		{
			if (GetCellItem(neighbor) == LivingCellIdx)
			{
				livingNeighborCount++;
			}
		}

		if (GetCellItem(cell) == LivingCellIdx)
		{
			if (livingNeighborCount < 2 || livingNeighborCount > 3) 
			{
				return GhostCellIdx;
			}
			return LivingCellIdx;
		}
		if (livingNeighborCount == 3)
		{
			return LivingCellIdx;
		}
		return GhostCellIdx;
	}

	private List<Vector3I> GetNeighborCells(Vector3I cell)
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
