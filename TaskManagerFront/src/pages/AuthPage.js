import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { registerUser, loginUser } from '../api';

const AuthPage = ({ onLoginSuccess }) => {
  const [loginEmail, setLoginEmail] = useState('');
  const [signupEmail, setSignupEmail] = useState('');

  const [loginPassword, setLoginPassword] = useState('');
  const [signupPassword, setSignupPassword] = useState('');

  const [userName, setUserName] = useState('');

  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const response = await loginUser({ 
        Email: loginEmail, 
        Password: loginPassword 
      });
      console.log('Login Success:', response.data);
      localStorage.setItem('token', response.data.token);
      onLoginSuccess();
      setLoginEmail('')
      setLoginPassword('')
      navigate('/board');
    } catch (error) {
      console.error('Login Error:', error.response?.data || error.message);
    }
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      const response = await registerUser({ 
        Email: signupEmail, 
        UserName: userName, 
        Password: signupPassword 
      });
      console.log('Register Success:', response.data);
      setSignupEmail('')
      setSignupPassword('')
      setUserName('')
      navigate('/board')
    } catch (error) {
      console.error('Register Error:', error.response?.data || error.message);
    }
  };

  return (
    <div>
      <form onSubmit={handleLogin}>
        <h2>Login</h2>
        <input type="email" placeholder="Email" value={loginEmail} onChange={(e) => setLoginEmail(e.target.value)} required />
        <input type="password" placeholder="Password" value={loginPassword} onChange={(e) => setLoginPassword(e.target.value)} required />
        <button type="submit">Log in</button>
      </form>

      <hr />

      <form onSubmit={handleRegister}>
        <h2>Signup</h2>
        <input type="text" placeholder="Username" value={userName} onChange={(e) => setUserName(e.target.value)} required />
        <input type="email" placeholder="Email" value={signupEmail} onChange={(e) => setSignupEmail(e.target.value)} required />
        <input type="password" placeholder="Password" value={signupPassword} onChange={(e) => setSignupPassword(e.target.value)} required />
        <button type="submit">Sign up</button>
      </form>
    </div>
  );
};

export default AuthPage;