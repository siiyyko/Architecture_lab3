import React, { useState, useEffect } from 'react';
import './../styles/TaskModal.css';
import Comments from './Comments'
import InlineEdit from './InlineEdit';

const TaskModal = ({ task, onClose, onSave, usersMap, currentUser }) => {
  const [formData, setFormData] = useState(null);

  useEffect(() => {
    if (task) {
      setFormData(task);
    }
  }, [task]);

  if (!task) return null;

  const handleChange = (e) => {
    const { name, value } = e.target;
    
    let finalValue = value;
    if (name === 'status' || name === 'priority') {
      finalValue = parseInt(value, 10);
    }
    if (name === 'assigneeId' && value === 'null') {
      finalValue = null;
    }

    setFormData({
      ...formData,
      [name]: finalValue
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSave(formData);
  };

  const userList = Array.from(usersMap.entries()).map(([id, name]) => ({ id, name }));

  if (!formData) return null;

  const isNew = !formData.id;

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <form className="modal-content" onClick={(e) => e.stopPropagation()} onSubmit={handleSubmit}>
        <button type="button" className="modal-close-btn" onClick={onClose}>×</button>
        
        <InlineEdit
          as="input"
          name="name"
          className="modal-title-input"
          value={formData.name}
          onChange={handleChange}
        />
        
        {!isNew && <p>{formData.codeName}</p>}
        
        <div className="modal-body">
          <div className="task-details">
            <h3>Description</h3>
            <InlineEdit
              as="textarea"
              name="description"
              value={formData.description || ''}
              onChange={handleChange}
              placeholder="Add description..."
            />
            
            <div className="task-details">
              <div className='task-detail'>
                <label>Status</label>
                <select name="status" value={formData.status} onChange={handleChange}>
                  <option value={0}>To Do</option>
                  <option value={1}>In Progress</option>
                  <option value={2}>In Review</option>
                  <option value={3}>Done</option>
                </select>
              </div>

              <div className='task-detail'>
                <label>Assignee</label>
                <select name="assigneeId" value={formData.assigneeId || 'null'} onChange={handleChange}>
                  <option value="null">Not assigned</option>
                  {userList.map(user => (
                    <option key={user.id} value={user.id}>{user.name}</option>
                  ))}
                </select>
              </div>
              
              <div className='task-detail'>
                <label>Reporter</label>
                <p>{usersMap.get(formData.reporterId) || 'Unknown'}</p>
              </div>
            </div>
          </div>
        </div>
        <div className="task-comments">
          <h3>Comments</h3>
          {!isNew ? (
            <Comments 
              taskId={formData.id}
              usersMap={usersMap}
              currentUser={currentUser}
            />
          ) : (
            <p>Comments will be displayed after task creation.</p>
          )}
        </div>

        <div className="modal-footer">
          <button type="submit" className="save-button">
            {isNew ? 'Create' : 'Save'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default TaskModal;