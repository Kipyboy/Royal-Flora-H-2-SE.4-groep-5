 'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/Registreren.css';
// `setToken` slaat de auth token op na succesvolle registratie/login.
import { setToken } from '../utils/auth';
// Data transfer object type voor de response van het register endpoint
import type { RegisterResponseDTO } from '../utils/dtos';
// Basis-URL naar de backend API
import { API_BASE_URL } from '../config/api';


export default function BedrijfRegistreren() {
    const router = useRouter();

    // Bij mount controleren we of er eerder ingevulde registratiegegevens in sessionStorage
    // zijn achtergebleven (bijv. vanuit een multi-step formulier). Als die er zijn, laden we
    // ze in state zodat we ze kunnen samenvoegen met dit formulier voordat we naar de backend posten.
    useEffect(() => {
        try {
            const raw = sessionStorage.getItem('registrationForm');
            if (!raw) return;
            const saved = JSON.parse(raw);

            
            setSavedRegistration(saved);

            
            sessionStorage.removeItem('registrationForm');
        } catch (err) {
            console.warn('Could not read registrationForm from sessionStorage', err);
        }
    }, []);

    // `formData` bevat de bedrijfsvelden die op dit formulier ingevuld worden
    const [formData, setFormData] = useState({
        naam: '',
        postcode: '',
        adress: ''
    });
    // `savedRegistration` bevat eerder ingevulde persoonlijke gegevens uit de eerste registratiestap
    // (gelezen uit sessionStorage in de useEffect hierboven)
    const [savedRegistration, setSavedRegistration] = useState<any | null>(null);
    // `errors` bevat validatie- of serverfouten die we onder de velden tonen
    const [errors, setErrors] = useState({
        naam: '',
        postcode: '',
        adress: ''
    });

    // Algemene onChange handler voor inputs: update `formData` en wis eventuele foutmelding
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

    // Voer eenvoudige client-side validatie uit voordat we naar de server sturen
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

    // Stuurt de volledige registratiegegevens naar de backend. Voegt data uit de eerste
    // registratiestap (persoonlijke gegevens) samen met dit bedrijfsformulier en verwerkt
    // succes (token opslaan, minimale gebruikersgegevens bewaren, doorsturen) en verschillende foutgevallen.
    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            if (!savedRegistration) {
                // data van de vorige stap zou present moeten zijn, als die er niet is gebruiker terugsturen.
                alert('Ongeldige registratiegegevens. Keer terug naar het registratieformulier.');
                router.push('/registreren');
                return;
            }

            // Sla de eerder ingevulde registratiegegevens (stap 1) samen met de bedrijfsvelden in de payload op.
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
                // Bij succes retourneert de backend een token en gebruiker, vergelijkbaar met bij inloggen
                let data: RegisterResponseDTO | null = null;
                try {
                    data = await response.json();
                } catch (jsonError) {
                    // Server gaf geen geldige JSON terug; informeer de gebruiker en doorsturen naar inloggen
                    alert('Registratie succesvol, maar onjuiste server response. Log in handmatig.');
                    router.push('/login');
                    return;
                }

                // Sla token op en bewaar een minimaal user object in localStorage
                const token = (data?.token || data?.token) as string | undefined;
                const user = (data?.user || data?.user) || null;
                if (token) setToken(token);
                if (user) {
                    localStorage.setItem('user', JSON.stringify({ id: user.id, username: user.username, email: user.email, role: user.role }));
                }

                alert('Registratie succesvol! Je bent nu ingelogd.');
                router.push('/homepage');
            } else {
                // Probeer de serverfout leesbaar te maken voor de gebruiker (JSON of plain text)
                try {
                    const errorText = await response.text();
                    let errorMessage = 'Onbekende fout';
                    try {
                        const error = JSON.parse(errorText);
                        errorMessage = error.message || errorMessage;
                    } catch {
                        // Als het geen JSON is, gebruik dan de tekst of een generiek bericht
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
                {/* Skip link voor toegankelijkheid: spring direct naar de hoofdinhoud */}
                <a href="#main" className="skip-link">Spring naar hoofdinhoud</a>
            
            {/* Formulier voor bedrijfgegevens. onSubmit stuurt alles samen met eerder opgeslagen persoonlijke data */}
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
