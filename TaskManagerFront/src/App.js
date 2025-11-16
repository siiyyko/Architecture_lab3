import React, { useState, useEffect } from 'react';
import AuthPage from './pages/AuthPage';
import BoardView from './pages/BoardView';

import './App.css';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';

const AppWrapper = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
try {
        const decodedToken = jwtDecode(token); 
        setCurrentUser({ id: decodedToken.sub, name: decodedToken.name });
        setIsAuthenticated(true);
      } catch (e) {
        console.error("Invalid token");
        handleLogout();
      }
    }
  }, []);

  const handleLoginSuccess = () => {
    const token = localStorage.getItem('token');
    const decodedToken = jwtDecode(token);
    setCurrentUser({ id: decodedToken.sub, name: decodedToken.name });
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setIsAuthenticated(false);
    setCurrentUser(null);
    navigate('/login');
  };

  return (
    <div className="App">
      <header className="App-header">
        <div className="Main-Header-Strip">
          <h1 className="Main-Header">Task Manager</h1>
          {isAuthenticated && (
            <span className="Main-Header">Welcome, {currentUser?.name}!</span>
          )}
          {isAuthenticated && (
            <button onClick={handleLogout} className="logout-button">
              Log out
            </button>
          )}
        </div>
        <Routes>
          <Route 
            path="/login" 
            element={!isAuthenticated ? 
              <AuthPage onLoginSuccess={handleLoginSuccess} /> :
              <Navigate to="/board" />
            } 
          />

          <Route 
            path="/board" 
            element={isAuthenticated ? 
              <BoardView currentUser={currentUser} /> :
              <Navigate to="/login" />
            } 
          />

          <Route path="*" element={<Navigate to="/login" />} />
        </Routes>
      </header>
    </div>
  );
}

function App() {
  return (
    <Router>
      <AppWrapper/>
    </Router>
  )
}

export default App;