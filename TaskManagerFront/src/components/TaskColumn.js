import React from 'react';
import { Droppable } from 'react-beautiful-dnd';
import TaskCard from './TaskCard';

const TaskColumn = ({ title, tasks, usersMap, onTaskClick, id }) => {
  return (
    <div className="task-column">
      <h3>{title}</h3>
      <Droppable droppableId={id}> 
        {(provided) => (
          <div 
            className="task-list"
            ref={provided.innerRef}
            {...provided.droppableProps}
          >
            {tasks.map((task, index) => (
              <TaskCard 
                key={task.id} 
                task={task} 
                usersMap={usersMap} 
                onClick={onTaskClick}
                index={index}
              />
            ))}
            {provided.placeholder}
          </div>
        )}
      </Droppable>
    </div>
  );
};

export default TaskColumn;


{/* <div className="task-list">
        {tasks.map(task => (
          <TaskCard 
            key={task.id} 
            task={task} 
            usersMap={usersMap} 
            onClick={onTaskClick}
          />
        ))}
      </div> */}