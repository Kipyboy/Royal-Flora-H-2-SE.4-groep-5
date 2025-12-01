'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import '../../styles/Login.css';
import { setToken, setUser } from '../utils/auth';
import type { LoginResponseDTO } from '../utils/dtos';

export default function Login() {
  const router = useRouter();
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [errors, setErrors] = useState({ email: '', password: '' });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    setErrors(prev => ({ ...prev, [name]: '' }));
  };

  const validateForm = () => {
    const newErrors = { email: '', password: '' };
    let isValid = true;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!formData.email.trim()) {
      newErrors.email = 'Email is verplicht';
      isValid = false;
    } else if (!emailRegex.test(formData.email)) {
      newErrors.email = 'Ongeldig email adres';
      isValid = false;
    }

    if (!formData.password) {
      newErrors.password = 'Wachtwoord is verplicht';
      isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!validateForm()) return;

    try {
      const response = await fetch('http://localhost:5156/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: formData.email, password: formData.password }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        alert(errorData?.message || 'Inloggen mislukt');
        return;
      }

      const data: LoginResponseDTO = await response.json();
      if (data.success) {
        if (data.token) setToken(data.token);
        if (data.user) {
          setUser({
            id: data.user.id,
            username: data.user.username,
            email: data.user.email,
            role: data.user.role,
            KVK: data.user.KVK || "", // ✅ include KVK
          });
        }
        alert('Inloggen succesvol!');
        router.push('/homepage');
      } else {
        alert(data.message || 'Onjuiste email of wachtwoord');
      }
    } catch (error) {
      console.error(error);
      alert('Kan geen verbinding maken met de server.');
    }
  };

  return (
    <div className="login-page">
      <main id="main">
        <a href="#main" className="skip-link">Spring naar hoofdinhoud</a>
        <form onSubmit={handleSubmit} aria-labelledby="login-title">
          <h1 id="login-title">Inloggen</h1>

          <div className="form-group">
            <label htmlFor="email">Email adres</label>
            <input type="email" id="email" name="email" autoComplete="email" value={formData.email} onChange={handleChange} required />
            {errors.email && <div className="error-message">{errors.email}</div>}
          </div>

          <div className="form-group">
            <label htmlFor="password">Wachtwoord</label>
            <input type="password" id="password" name="password" autoComplete="current-password" value={formData.password} onChange={handleChange} required />
            {errors.password && <div className="error-message">{errors.password}</div>}
          </div>

          <Link href="/forgot-password" className="forgot-password">Wachtwoord vergeten?</Link>

          <button type="submit" className="login-button">Inloggen</button>
        </form>
      </main>
    </div>
  );
}
