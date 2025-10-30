'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/Registreren.css';

export default function Registreren() {
    const router = useRouter();
    const [formData, setFormData] = useState({
        voornaam: '',
        achternaam: '',
        telefoon: '',
        email: '',
        password: '',
        confirmPassword: ''
    });
    const [errors, setErrors] = useState({
        voornaam: '',
        achternaam: '',
        telefoon: '',
        email: '',
        password: '',
        confirmPassword: ''
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
        // error leeg halen wanneer je typt
        setErrors(prev => ({
            ...prev,
            [name]: ''
        }));
    };

    const validateForm = () => {
        const newErrors = {
            voornaam: '',
            achternaam: '',
            telefoon: '',
            email: '',
            password: '',
            confirmPassword: ''
        };
        let isValid = true;

        // Kijken of iets leeg is of niet van tevoren klopt
        if (!formData.voornaam.trim()) {
            newErrors.voornaam = 'Voornaam is verplicht';
            isValid = false;
        }

        if (!formData.achternaam.trim()) {
            newErrors.achternaam = 'Achternaam is verplicht';
            isValid = false;
        }

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
        } else if (formData.password.length < 6) {
            newErrors.password = 'Wachtwoord moet minimaal 6 karakters zijn';
            isValid = false;
        }

        if (!formData.confirmPassword) {
            newErrors.confirmPassword = 'Herhaal wachtwoord is verplicht';
            isValid = false;
        } else if (formData.password !== formData.confirmPassword) {
            newErrors.confirmPassword = 'Wachtwoorden komen niet overeen';
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
            const response = await fetch('http://localhost:5156/api/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    voorNaam: formData.voornaam,
                    achterNaam: formData.achternaam,
                    telefoonnummer: formData.telefoon,
                    e_mail: formData.email,
                    wachtwoord: formData.password
                }),
            });

            if (response.ok) {
                alert('Registratie succesvol!');
                router.push('/login');
            } else {
                const error = await response.json();
                alert(`Registratie mislukt: ${error.message || 'Onbekende fout'}`);
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Er is een fout opgetreden bij het registreren');
        }
    };

    return (
        <main id="main">
            <a href="#main" className="skip-link">Spring naar hoofdinhoud</a>
            
            <form onSubmit={handleSubmit} aria-labelledby="register-title">
                <h1 id="register-title">Registreren</h1>
                
                <fieldset className="name-row">
                    <legend className="visually-hidden">Persoonlijke gegevens</legend>
                    <div className="form-group half-width">
                        <label htmlFor="voornaam">Voornaam</label>
                        <input 
                            type="text" 
                            id="voornaam" 
                            name="voornaam" 
                            required 
                            aria-describedby="voornaam-error"
                            autoComplete="given-name"
                            value={formData.voornaam}
                            onChange={handleChange}
                        />
                        {errors.voornaam && (
                            <div id="voornaam-error" className="error-message" aria-live="polite">
                                {errors.voornaam}
                            </div>
                        )}
                    </div>
                    
                    <div className="form-group half-width">
                        <label htmlFor="achternaam">Achternaam</label>
                        <input 
                            type="text" 
                            id="achternaam" 
                            name="achternaam" 
                            required 
                            aria-describedby="achternaam-error"
                            autoComplete="family-name"
                            value={formData.achternaam}
                            onChange={handleChange}
                        />
                        {errors.achternaam && (
                            <div id="achternaam-error" className="error-message" aria-live="polite">
                                {errors.achternaam}
                            </div>
                        )}
                    </div>
                </fieldset>
                
                <div className="form-group">
                    <label htmlFor="telefoon">Telefoonnummer <span className="optional">(optioneel)</span></label>
                    <input 
                        type="tel" 
                        id="telefoon" 
                        name="telefoon" 
                        aria-describedby="telefoon-error"
                        autoComplete="tel"
                        value={formData.telefoon}
                        onChange={handleChange}
                    />
                    {errors.telefoon && (
                        <div id="telefoon-error" className="error-message" aria-live="polite">
                            {errors.telefoon}
                        </div>
                    )}
                </div>
                
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
                
                <fieldset>
                    <legend className="visually-hidden">Wachtwoord instellen</legend>
                    <div className="form-group">
                        <label htmlFor="password">Wachtwoord</label>
                        <input 
                            type="password" 
                            id="password" 
                            name="password" 
                            required 
                            aria-describedby="password-error"
                            autoComplete="new-password"
                            value={formData.password}
                            onChange={handleChange}
                        />
                        {errors.password && (
                            <div id="password-error" className="error-message" aria-live="polite">
                                {errors.password}
                            </div>
                        )}
                    </div>
                    
                    <div className="form-group">
                        <label htmlFor="confirm-password">Herhaal wachtwoord</label>
                        <input 
                            type="password" 
                            id="confirm-password" 
                            name="confirmPassword" 
                            required 
                            aria-describedby="confirm-password-error"
                            autoComplete="new-password"
                            value={formData.confirmPassword}
                            onChange={handleChange}
                        />
                        {errors.confirmPassword && (
                            <div id="confirm-password-error" className="error-message" aria-live="polite">
                                {errors.confirmPassword}
                            </div>
                        )}
                    </div>
                </fieldset>
                
                <button type="submit" className="register-button">Registreren</button>
            </form>
        </main>
    );
}
