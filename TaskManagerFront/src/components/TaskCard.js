import React from 'react';
import { Draggable } from 'react-beautiful-dnd';

const TaskCard = ({ task, usersMap, onClick, index }) => {
  
  const assigneeName = task.assigneeId ? usersMap.get(task.assigneeId) : 'Not assigned';

  const handleClick = () => {
    onClick(task);
  };

  return (
    <Draggable draggableId={task.id} index={index}>
      {(provided) => (
        <div 
          className="task-card" 
          onClick={handleClick}
          {...provided.draggableProps}
          {...provided.dragHandleProps}
          ref={provided.innerRef}
        >
          <h4>{task.name}</h4>
          <p>({task.codeName})</p>
          <div className="task-card-assignee">
            {assigneeName}
          </div>
        </div>
      )}
    </Draggable>
  );
};

export default TaskCard;