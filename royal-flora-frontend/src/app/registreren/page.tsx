'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/Registreren.css';
import { setToken } from '../utils/auth';
import type { RegisterResponseDTO } from '../utils/dtos';


export default function Registreren() {
    const router = useRouter();
    const [formData, setFormData] = useState({
        voornaam: '',
        achternaam: '',
        telefoon: '',
        email: '',
        password: '',
        confirmPassword: '',
        kvk: '',
        accountType: 'klant',
        postcode: '',
        adress: ''
    });
    const [errors, setErrors] = useState({
        voornaam: '',
        achternaam: '',
        telefoon: '',
        email: '',
        password: '',
        confirmPassword: '',
        kvk: '',
        postcode: '',
        adress: ''
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
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
            confirmPassword: '',
            kvk: '',
            postcode: '',
            adress: ''
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

        if (!formData.email.trim()) {
            newErrors.email = 'Email is verplicht';
            isValid = false;
        }

        if (!formData.password) {
            newErrors.password = 'Wachtwoord is verplicht';
            isValid = false;
        } else {
            // At least 8 characters, at least one digit and one special character
            const pwdRegex = /^(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]).{8,}$/;
            if (!pwdRegex.test(formData.password)) {
                newErrors.password = 'Wachtwoord moet minimaal 8 tekens bevatten, inclusief een cijfer en een speciaal teken';
                isValid = false;
            }
        }

        if (!formData.confirmPassword) {
            newErrors.confirmPassword = 'Herhaal wachtwoord is verplicht';
            isValid = false;
        } else if (formData.password !== formData.confirmPassword) {
            newErrors.confirmPassword = 'Wachtwoorden komen niet overeen';
            isValid = false;
        }
        if (!formData.kvk.trim()) {
            newErrors.kvk = 'KvK-nummer is verplicht';
            isValid = false;
        } else if (!/^\d{8}$/.test(formData.kvk)) {
            newErrors.kvk = 'KvK-nummer moet 8 cijfers bevatten';
            isValid = false;
        }
        if (!formData.postcode.trim()) {
            newErrors.postcode = 'Postcode is verplicht';
            isValid = false;
        }
        if (!formData.adress.trim()) {
            newErrors.adress = 'Adres is verplicht';
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
                    wachtwoord: formData.password,
                    kvkNummer: formData.kvk,
                    postcode: formData.postcode,
                    adres: formData.adress,
                    accountType: formData.accountType

                }),
            });

            if (response.ok) {
                // On success, backend returns token and user similar to login
                let data: RegisterResponseDTO | null = null;
                try {
                    data = await response.json();
                } catch (jsonError) {
                    alert('Registratie succesvol, maar onjuiste server response. Log in handmatig.');
                    router.push('/login');
                    return;
                }

                const token = (data?.token || data?.Token) as string | undefined;
                const user = (data?.user || data?.User) || null;
                if (token) setToken(token);
                if (user) {
                    localStorage.setItem('user', JSON.stringify({ id: user.id, username: user.username, email: user.email, role: user.role }));
                }

                alert('Registratie succesvol! Je bent nu ingelogd.');
                router.push('/homepage');
            } else {
                try {
                    const errorText = await response.text();
                    let errorMessage = 'Onbekende fout';
                    try {
                        const error = JSON.parse(errorText);
                        errorMessage = error.message || errorMessage;
                    } catch {
                        // If not JSON, just use the text or a generic message
                        errorMessage = errorText ? errorText.substring(0, 100) : errorMessage;
                    }
                    alert(`Registratie mislukt: ${errorMessage}`);
                } catch (error) {
                    alert('Registratie mislukt: Er is een fout opgetreden');
                }
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Er is een fout opgetreden bij het registreren');
        }
    };

    return (
        <div className="registreren-page">
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

                <div className='form-group'>
                    <label htmlFor='kvk'>KvK-nummer</label>
                    <input 
                        type="text" 
                        id="kvk" 
                        name="kvk" 
                        required 
                        aria-describedby="kvk-error"
                        autoComplete="off"
                        value={formData.kvk}
                        onChange={handleChange}
                    />
                    {errors.kvk && (
                        <div id="kvk-error" className="error-message" aria-live="polite">
                            {errors.kvk}
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
                <fieldset>
                    <div className='form-group'>
                        <label htmlFor='postcode'>Postcode</label>
                        <input 
                            type="text" 
                            id="postcode"
                            name="postcode" 
                            required 
                            aria-describedby="postcode-error"
                            autoComplete="postal-code"
                            value={formData.postcode}
                            onChange={handleChange}
                        />
                        
                    {errors.postcode && (
                        <div id="postcode-error" className="error-message" aria-live="polite">
                            {errors.postcode}
                        </div>
                    )}
                    </div>
                    <div className='form-group'>
                        <label htmlFor='adress'>Adres</label>
                        <input
                            type="text"
                            id="adress"
                            name="adress"
                            required 
                            aria-describedby="adress-error"
                            autoComplete="address-line1"
                            value={formData.adress}
                            onChange={handleChange}
                        />
                        {errors.adress && (
                            <div id="adress-error" className="error-message" aria-live="polite">
                                {errors.adress}
                            </div>
                        )}
                    </div>
                </fieldset>
                <div className='form-group'>
                    <label htmlFor='account-type'>Account type</label>
                    <select 
                        id='account-type' 
                        name='accountType'
                        value={formData.accountType}
                        onChange={handleChange}
                    >
                        <option value='klant'>Inkoper</option>
                        <option value='bedrijf'>Aanvoerder</option>
                    </select>
                </div>
                
                <button type="submit" className="register-button">Registreren</button>
            </form>
        </main>
        </div>
    );
}
