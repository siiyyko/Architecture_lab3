import React, { useState, useEffect } from 'react';
import { getTasks, getUsers, updateTaskStatus, updateTask, createTask } from '../api';
import TaskColumn from '../components/TaskColumn';
import TaskModal from '../components/TaskModal';
import { DragDropContext } from 'react-beautiful-dnd';
import './../styles/BoardView.css'; 

const statusMap = {
  "todo": 0,
  "inprogress": 1,
  "inreview": 2,
  "done": 3
};
const getColumnIdByStatus = (status) => Object.keys(statusMap).find(key => statusMap[key] === status);

const BoardView = ({ currentUser }) => {
  const [tasks, setTasks] = useState([]);
  const [usersMap, setUsersMap] = useState(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedTask, setSelectedTask] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);

        const [tasksResponse, usersResponse] = await Promise.all([
          getTasks(),
          getUsers()
        ]);

        setTasks(tasksResponse.data);

        const usersMap = new Map();
        usersResponse.data.forEach(user => {
          usersMap.set(user.id, user.userName);
        });
        setUsersMap(usersMap);

      } catch (err) {
        console.error("Помилка завантаження даних:", err);
        setError("Не вдалося завантажити дані. Перевірте консоль.");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleOpenModal = (task) => {
    setSelectedTask(task);
  };

  const handleCloseModal = () => {
    setSelectedTask(null);
  };

  const handleOpenCreateModal = () => {
    setSelectedTask({
      name: "New task",
      description: "",
      status: 0,
      priority: 1,
      reporterId: currentUser.id,
      assigneeId: null
    });
  };

  const handleSaveTask = async (taskToSave) => {
    try {
      if (taskToSave.id) {
        const response = await updateTask(taskToSave.id, taskToSave);
        setTasks(tasks.map(t => t.id === taskToSave.id ? response.data : t));
      } else {
        const response = await createTask(taskToSave);
        setTasks([...tasks, response.data]);
      }
      handleCloseModal();
    } catch (err) {
      console.error("Error saving task:", err);
      alert("Error saving task.");
    }
  };

  const handleTaskUpdate = async (updatedTask) => {
    if (updatedTask.id) {
      try {
        const response = await updateTask(updatedTask.id, updatedTask);
        setTasks(tasks.map(t => t.id === updatedTask.id ? response.data : t));
        setSelectedTask(response.data);
      } catch (err) {
        console.error("Error editing task", err);
      }
    } else {
      try {
        const response = await createTask(updatedTask);
        setTasks([...tasks, response.data]);
        setSelectedTask(response.data);
      } catch (err) {
        console.error("Error creating task", err);
      }
    }
  };

  const onDragEnd = (result) => {
    const { destination, source, draggableId } = result;

    if (!destination) return;

    if (destination.droppableId === source.droppableId &&
        destination.index === source.index) {
      return;
    }

    const task = tasks.find(t => t.id === draggableId);
    
    const newStatus = statusMap[destination.droppableId];

    const newTasks = tasks.map(t => 
      t.id === draggableId ? { ...t, status: newStatus } : t
    );
    setTasks(newTasks);

    updateTaskStatus(draggableId, newStatus)
      .catch(err => {
        console.error("Error changing status!", err);
        setTasks(tasks);
        alert("Could not change status.");
      });
  };

  const todoTasks = tasks.filter(t => t.status === 0);
  const inProgressTasks = tasks.filter(t => t.status === 1);
  const inReviewTasks = tasks.filter(t => t.status === 2);
  const doneTasks = tasks.filter(t => t.status === 3);

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <button onClick={handleOpenCreateModal} className="create-task-btn">
        + Create Task
      </button>
      <div className="board-view">
        <TaskColumn title="To Do" tasks={todoTasks} usersMap={usersMap} onTaskClick={handleOpenModal} />
        <TaskColumn title="In Progress" tasks={inProgressTasks} usersMap={usersMap} onTaskClick={handleOpenModal} />
        <TaskColumn title="In Review" tasks={inReviewTasks} usersMap={usersMap} onTaskClick={handleOpenModal} />
        <TaskColumn title="Done" tasks={doneTasks} usersMap={usersMap} onTaskClick={handleOpenModal} />
        {selectedTask && (
        <TaskModal 
          task={selectedTask}
          onClose={handleCloseModal}
          onSave={handleSaveTask}
          usersMap={usersMap}
          currentUser={currentUser}
        />
      )}
      </div>
    </DragDropContext>
  );
};

export default BoardView;