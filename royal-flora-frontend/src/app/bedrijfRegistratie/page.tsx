 'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/Registreren.css';
import { setToken } from '../utils/auth';
import type { RegisterResponseDTO } from '../utils/dtos';
import { API_BASE_URL } from '../config/api';


export default function BedrijfRegistreren() {
    const router = useRouter();

    useEffect(() => {
        try {
            const raw = sessionStorage.getItem('registrationForm');
            if (!raw) return;
            const saved = JSON.parse(raw);

            // keep a copy in state so we can merge and post it later
            setSavedRegistration(saved);

            // remove the saved item so it doesn't persist longer than needed
            sessionStorage.removeItem('registrationForm');
        } catch (err) {
            console.warn('Could not read registrationForm from sessionStorage', err);
        }
    }, []);

    const [formData, setFormData] = useState({
        naam: '',
        postcode: '',
        adress: ''
    });
    const [savedRegistration, setSavedRegistration] = useState<any | null>(null);
    const [errors, setErrors] = useState({
        naam: '',
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
            naam: '',
            postcode: '',
            adress: ''
        };
        let isValid = true;

        // Kijken of iets leeg is of niet van tevoren klopt
        if (!formData.naam.trim()) {
            newErrors.naam = 'Bedrijfnaam is verplicht';
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
            if (!savedRegistration) {
                alert('Ongeldige registratiegegevens. Keer terug naar het registratieformulier.');
                router.push('/registreren');
                return;
            }

            // Merge saved registration data from the first form with the company fields.
            const payload = {
                voorNaam: savedRegistration.voornaam,
                achterNaam: savedRegistration.achternaam,
                telefoonnummer: savedRegistration.telefoon,
                e_mail: savedRegistration.email,
                wachtwoord: savedRegistration.password,
                kvkNummer: savedRegistration.kvk,
                postcode: savedRegistration.postcode,
                adres: savedRegistration.adress,
                accountType: savedRegistration.accountType,
                bedrijfPostcode: formData.postcode,
                bedrijfAdres: formData.adress,
                bedrijfNaam: formData.naam
            };

            const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(payload),
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

                const token = (data?.token || data?.token) as string | undefined;
                const user = (data?.user || data?.user) || null;
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
            
            <form onSubmit={handleSubmit} aria-labelledby="bedrijf-register-title">
                <h1 id="bedrijf-register-title">Nieuw Bedrijf Registreren</h1>
                <h2>Het door u opgegeven KvK nummer komt niet voor in ons systeem, maak een nieuw bedrijf aan.</h2>
                
                <fieldset className="name-row">
                    <legend className="visually-hidden">Persoonlijke gegevens</legend>
                    <div className="form-group half-width">
                        <label htmlFor="naam">Naam</label>
                        <input 
                            type="text" 
                            id="naam" 
                            name="naam" 
                            required 
                            aria-describedby="naam-error"
                            value={formData.naam}
                            onChange={handleChange}
                        />
                        {errors.naam && (
                            <div id="naam-error" className="error-message" aria-live="polite">
                                {errors.naam}
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
                
                <button type="submit" className="register-button">Registreren</button>
            </form>
        </main>
        </div>
    );
}
