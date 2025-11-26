'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import '../../styles/Login.css';
import { setToken } from '../utils/auth';
import type { LoginResponseDTO } from '../utils/dtos';

export default function Login() {
    const router = useRouter();
    const [formData, setFormData] = useState({
        email: '',
        password: ''
    });
    const [errors, setErrors] = useState({
        email: '',
        password: ''
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
        // Haalt de error weg wanneer je typed
        setErrors(prev => ({
            ...prev,
            [name]: ''
        }));
    };

    const validateForm = () => {
        const newErrors = {
            email: '',
            password: ''
        };
        let isValid = true;

        // Email checken of het een email is en dat het niet leeg is
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!formData.email.trim()) {
            newErrors.email = 'Email is verplicht';
            isValid = false;
        } else if (!emailRegex.test(formData.email)) {
            newErrors.email = 'Ongeldig email adres';
            isValid = false;
        }

        // Wachtwoord Checken of het niet leeg is
        if (!formData.password) {
            newErrors.password = 'Wachtwoord is verplicht';
            isValid = false;
        }

        setErrors(newErrors);
        return isValid;
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            const response = await fetch('http://localhost:5156/api/auth/login', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Email: formData.email,
                    Password: formData.password
                }),
            });

            // Network error (server unreachable)
            if (!response) {
                alert('Kan geen verbinding maken met de server. Controleer of de backend draait.');
                return;
            }

            // 401 Unauthorized: wrong credentials
            if (response.status === 401) {
                let errorData = null;
                try {
                    errorData = await response.json();
                } catch {}
                alert(`Inloggen mislukt: ${errorData?.Message || errorData?.message || 'Onjuiste email of wachtwoord'}`);
                return;
            }

            // Other non-200 errors
            if (!response.ok) {
                alert('Er is een fout opgetreden bij het inloggen. Probeer het later opnieuw.');
                return;
            }

            let data: LoginResponseDTO | null = null;
            try {
                data = await response.json();
            } catch (jsonError) {
                alert('Server gaf geen geldige response terug.');
                return;
            }

            if (data && (data.Success === true || (data as any).success === true)) {
                // Prefer token from response (token or Token)
                const token = (data.token || data.Token) as string | undefined;
                const user = (data.user || data.User) || null;
                if (token) {
                    setToken(token);
                }
                if (user) {
                    // store minimal user info
                    localStorage.setItem('user', JSON.stringify({ id: user.id, username: user.username, email: user.email, role: user.role }));
                }
                alert('Inloggen succesvol!');
                router.push('/homepage');
            } else {
                alert(`Inloggen mislukt: ${data?.Message || (data as any)?.message || 'Onjuiste email of wachtwoord'}`);
            }
        } catch (error: any) {
            console.error('Error:', error);
            alert('Kan geen verbinding maken met de server. Controleer of de backend draait.');
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
                    <input 
                        type="email" 
                        id="email" 
                        name="email" 
                        required 
                        aria-describedby="email-error"
                        autoComplete="email"
                        value={formData.email}
                        onChange={handleChange}
                    />
                    {errors.email && (
                        <div id="email-error" className="error-message" aria-live="polite">
                            {errors.email}
                        </div>
                    )}
                </div>
                
                <div className="form-group">
                    <label htmlFor="password">Wachtwoord</label>
                    <input 
                        type="password" 
                        id="password" 
                        name="password" 
                        required 
                        aria-describedby="password-error"
                        autoComplete="current-password"
                        value={formData.password}
                        onChange={handleChange}
                    />
                    {errors.password && (
                        <div id="password-error" className="error-message" aria-live="polite">
                            {errors.password}
                        </div>
                    )}
                </div>
                
                <Link href="/forgot-password" className="forgot-password">
                    Wachtwoord vergeten?
                </Link>
                
                <button type="submit" className="login-button">Inloggen</button>
            </form>
        </main>
        </div>
    );
}
