extends GridMap

@onready var tick_timer: Timer = $"../TickTimer"
enum CELL_STATES {
	EMPTY,
	LIVING,
	GHOST,
}
const EMPTY_CELL_IDX := -1
const LIVING_CELL_IDX := 0
const GHOST_CELL_IDX := 1


func _ready() -> void:
	self.tick_timer.timeout.connect(tick)


func tick() -> void:
	self.clean_ghost_cells()
	
	# Calculate changed cells
	var cell_changes: Dictionary[Vector3i, int] = {}
	for cell in self.get_used_cells():
		var cell_update = get_cell_update(cell)
		if not get_cell_state(cell) == cell_update:
			cell_changes[Vector3i(cell)] = cell_update
	
	# Apply changes
	for cell in cell_changes:
		if cell_changes[cell] == CELL_STATES.LIVING:
			self.set_cell_item(cell, LIVING_CELL_IDX)
		else:
			self.set_cell_item(cell, GHOST_CELL_IDX)


# Removes all ghost cells that do not neighbor a living cell
# Also replaces all empty cells around living cells with ghost cells
func clean_ghost_cells() -> void:
	var used_cells := self.get_used_cells()
	for cell in used_cells:
		if self.get_cell_state(cell) == CELL_STATES.GHOST:
			# Ghost cell
			var has_living_neighbor := false
			for neighbor in get_neighbor_cells(cell):
				if self.get_cell_state(neighbor) == CELL_STATES.LIVING:
					has_living_neighbor = true
					break
			if not has_living_neighbor:
				self.set_cell_item(cell, EMPTY_CELL_IDX)
		
		else:
			# Living cell
			for neighbor in get_neighbor_cells(cell):
				if self.get_cell_state(neighbor) == CELL_STATES.EMPTY:
					self.set_cell_item(neighbor, GHOST_CELL_IDX)


func get_cell_update(cell: Vector3i) -> int:
	var living_neighbor_count := 0
	for neighbor in self.get_neighbor_cells(cell):
		if self.get_cell_state(neighbor) == CELL_STATES.LIVING:
			living_neighbor_count += 1
			
	if self.get_cell_state(cell) == CELL_STATES.LIVING:
		if living_neighbor_count < 2:
			return CELL_STATES.GHOST
		elif living_neighbor_count > 3:
			return CELL_STATES.GHOST
		return CELL_STATES.LIVING
	else:
		if living_neighbor_count == 3:
			return CELL_STATES.LIVING
		return CELL_STATES.GHOST


func get_neighbor_cells(cell: Vector3i) -> Array[Vector3i]:
	var neighbors: Array[Vector3i] = []
	for x in range(-1, 2):
		for y in range(-1, 2):
			for z in range(-1, 2):
				if x == 0 and y == 0 and z == 0:
					continue
				neighbors.append(cell + Vector3i(x, y, z))
	return neighbors


func get_cell_state(cell: Vector3i) -> int:
	var cell_item := get_cell_item(cell)
	if cell_item == LIVING_CELL_IDX:
		return CELL_STATES.LIVING
	elif cell_item == GHOST_CELL_IDX:
		return CELL_STATES.GHOST
	return CELL_STATES.EMPTY
