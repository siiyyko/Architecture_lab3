import React, { useState, useEffect } from 'react';
import { getCommentsForTask, postCommentAsync, postCommentSync } from '../api';
import './../styles/Comments.css';

const Comments = ({ taskId, currentUser, usersMap }) => {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!taskId) return;

    const fetchComments = async () => {
      try {
        setLoading(true);
        const response = await getCommentsForTask(taskId);
        setComments(response.data);
      } catch (err) {
        console.error("Error loading comments:", err);
        setError("Error loading comments.");
      } finally {
        setLoading(false);
      }
    };

    fetchComments();
  }, [taskId]);

  const handleSubmit = async (e, submissionType) => {
    e.preventDefault();
    if (!newComment.trim()) return;

    const commentData = {
      taskId: taskId,
      authorId: currentUser.id,
      content: newComment,
    };

    console.log(`Sending (${submissionType}):`, commentData);
    
    try {
      let response;
      if (submissionType === 'async') {
        response = await postCommentAsync(commentData);
      } else {
        response = await postCommentSync(commentData);
      }

      setComments([...comments, response.data]);
      setNewComment('');
      
    } catch (err) {
      console.error(`Error sending (${submissionType}):`, err);
      alert("Error sending comment.");
    }
  };
  
  const renderComments = () => {
    if (loading) return <div>Loading...</div>;
    if (error) return <div className="error">{error}</div>;
    if (comments.length === 0) return <div>No comments yet.</div>;

    return (
      <div className="comments-list">
        {comments.map(comment => (
          <div key={comment.id} className="comment-item">
            <strong>{usersMap.get(comment.authorId) || 'Невідомий'}</strong>
            <p>{comment.content}</p>
            <span>{new Date(comment.createdAt).toLocaleString()}</span>
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className="comments-section">
      {renderComments()}
      
      <form className="comment-form">
        <textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Write a comment..."
        />
        <div className="comment-buttons">
          <button onClick={(e) => handleSubmit(e, 'async')}>
            Send (Async)
          </button>
          <button onClick={(e) => handleSubmit(e, 'sync')}>
            Send (Sync)
          </button>
        </div>
      </form>
    </div>
  );
};

export default Comments;