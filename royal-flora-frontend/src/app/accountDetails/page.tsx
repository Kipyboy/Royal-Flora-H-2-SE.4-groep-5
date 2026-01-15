'use client'
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/AccountDetails.css';
import Topbar from '../components/Topbar';
// Hulpfuncties voor sessie en authenticatie. Op dit moment wordt `getSessionData` niet gebruikt,
// maar hij kan later handig zijn voor client-side sessiebeheer.
import { getSessionData } from '../utils/sessionService';
// `logout` wordt hier als `clearAuth` geïmporteerd om sessiegegevens te wissen; `getAuthHeaders`
// haalt benodigde headers (o.a. Authorization token) voor API-aanroepen op.
import { logout as clearAuth, getAuthHeaders } from '../utils/auth';
// Basis-API URL voor fetch-oproepen
import { API_BASE_URL } from '../config/api';

// `UserDetails` definieert de vorm van de gebruikersgegevens die we in het formulier tonen en bijwerken.
interface UserDetails {
    voornaam: string;
    achternaam: string;
    email: string;
    telefoonnummer: string;
    adress: string;
    postcode: string;
    wachtwoord: string;
} 

const AccountDetails: React.FC = () => {
    // `router` gebruiken om navigatie op client-side af te handelen (redirects na logout, fouten, etc.).
    const router = useRouter();

    // `userDetails` houdt de huidige waarden van het formulier bij.
    const [userDetails, setUserDetails] = useState<UserDetails>({
        voornaam: '',
        achternaam: '',
        email: '',
        telefoonnummer: '',
        adress: '',
        postcode: '',
        wachtwoord: ''
    });

    // `disabledFields` regelt of een inputveld bewerkbaar is of niet (true = disabled).
    const [disabledFields, setDisabledFields] = useState<Record<string, boolean>>({
        voornaam: true,
        achternaam: true,
        email: true,
        telefoonnummer: true,
        adress: true,
        postcode: true,
        wachtwoord: true
    });

    // `errors` bevat foutmeldingen per veld en algemene fouten.
    const [errors, setErrors] = useState<Record<string, string>>({});
    // `loading` geeft aan of we nog wachten op de initiële data-fetch.
    const [loading, setLoading] = useState<boolean>(true); 

    
    // useEffect: bij het laden van de component halen we de gebruikersgegevens op van de API.
    React.useEffect(() => {
        const fetchSessionData = async () => {
            try {
                // Haal autorisatie headers op (bevat token). Zonder token redirecten we naar de startpagina.
                const authHeaders = getAuthHeaders();
                
                // Controleer of er een token aanwezig is
                if (!authHeaders.Authorization) {
                    console.error('No token found');
                    router.push('/'); // Geen geldige sessie: terug naar homepage
                    return;
                }

                // Ophalen van alle gebruikersinformatie voor het ingelogde account
                const response = await fetch(`${API_BASE_URL}/api/auth/allUserInfo`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...authHeaders,
                    }
                });

                if (response.ok) {
                    // Zet de ontvangen gegevens in de component state zodat het formulier values krijgt
                    const data = await response.json();
                    setUserDetails({
                        voornaam: data.voorNaam,
                        achternaam: data.achterNaam,
                        email: data.email || '',
                        telefoonnummer: data.telefoonnummer || '',
                        adress: data.adress || '',
                        postcode: data.postcode || '',
                        wachtwoord: '' // Wachtwoord niet invullen uit veiligheidsredenen
                    });
                } else {
                    console.error('Failed to fetch session data');
                    router.push('/');
                }
            } catch (error) {
                // Fout afhandelen en melding tonen
                console.error('Error fetching session:', error);
                setErrors({ general: 'Kon sessie gegevens niet ophalen' });
            } finally {
                // Stop de laad-indicator, ongeacht succes of fout
                setLoading(false);
            }
        };

        fetchSessionData();
    }, []); 


    // Retourneert een onChange handler voor een gegeven veldnaam zodat inputs het state updaten.
    const handleInputChange = (field: keyof UserDetails) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setUserDetails(prev => ({
            ...prev,
            [field]: e.target.value
        }));
    }; 

    // Toggle tussen bewerkbaar maken van een veld en opslaan wanneer het al bewerkbaar is.
    // Als het veld momenteel disabled is -> maak het editable; anders -> sla de wijziging op.
    const toggleField = (field: keyof UserDetails) => {
        if (disabledFields[field]) {
            setDisabledFields(prev => ({
                ...prev,
                [field]: false
            }));
        } else {
            handleSave(field);
        }
    }; 

    // Slaat een enkel veld op door een API-aanroep. Valideert eerst (bv. wachtwoordregels),
    // verifieert dat er een autorisatietoken is en behandelt fouten en response.
    const handleSave = async (field: keyof UserDetails) => {
        try {
            const authHeaders = getAuthHeaders();
            
            // Als er geen token is, terug naar de homepage (niet ingelogd)
            if (!authHeaders.Authorization) {
                router.push('/');
                return;
            }
            
            // Specifieke validatie voor wachtwoord: minimaal 8 tekens, minimaal 1 cijfer en 1 speciaal teken
            if (field == 'wachtwoord') {
                const pwdRegex = /^(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]).{8,}$/;
                if (!pwdRegex.test(userDetails[field])) {
                    return setErrors(prev => ({
                        ...prev,
                        [field]: 'Wachtwoord moet minstens 8 tekens bevatten, inclusief een cijfer en een speciaal teken'
                    }));
                }
            }

            // Verstuur de update naar de server
            const response = await fetch(`${API_BASE_URL}/api/auth/updateUserInfo`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        ...authHeaders
                    },
                    body: JSON.stringify({
                        Field: field,
                        Newvalue: userDetails[field]
                    })
                });
            
            if (response.ok) {
                // Bij succes: zet het veld weer op disabled en verwijder eventuele foutmelding
                setDisabledFields(prev => ({
                    ...prev,
                    [field]: true
                }));

                setErrors(prev => ({
                    ...prev,
                    [field]: ''
                }));
            } else {
                // HTTP-fout
                setErrors(prev => ({
                    ...prev,
                    [field]: 'Er ging iets mis bij het opslaan'
                }));
            }
        } catch (error) {
            // Netwerk/fetch-fout
            setErrors(prev => ({
                ...prev,
                [field]: 'Er ging iets mis bij het opslaan'
            }));
        }
    }; 

    // Logout: wis lokale auth-gegevens en navigeer terug naar startpagina
    const handleLogout = () => {
        clearAuth();
        router.push('/');
    }; 

    // Verwijder het account na bevestiging. Roept DELETE endpoint aan, en bij succes
    // worden lokale auth-gegevens gewist en de gebruiker omgeleid.
    const handleDeleteAccount = async () => {
        if (window.confirm('Weet je zeker dat je je account wilt verwijderen?')) {
            try{
                const authHeaders = getAuthHeaders();
                const response = await fetch(`${API_BASE_URL}/api/auth/deleteAccount`, {
                        method: 'DELETE',
                        headers: {
                            'Content-Type': 'application/json',
                            ...authHeaders,
                        }
                    })
                // Ongeacht response: clear auth en redirect (optie: hier ook response.ok controleren)
                clearAuth();
                router.push('/');
            } catch (error) {
                setErrors({ general: 'Kon account niet verwijderen' });
            }
        }   
    }; 

    

        // Toon een eenvoudige laadindicatie totdat de initiële data is opgehaald
        if (loading) {
        return (
            <div className='accountDetails-page'>
                <Topbar
                    currentPage="Account Details"
                    useSideBar={false}
                />
                <main id="main">
                    <p>Laden...</p>
                </main>
            </div>
        );
    }

    return (
        <div className='accountDetails-page'>
            
            <Topbar
                currentPage="Account Details"
                useSideBar={false}
            />
            

            <main id="main">
                <form onSubmit={e => e.preventDefault()} aria-labelledby="accountdetails-title">
                    <h1 id="accountdetails-title">Account Details</h1>

                    {/* Dynamisch genereren van form-velden: sleutel -> label. Dit maakt de code compact
                        en zorgt dat elk veld dezelfde logica gebruikt (disabled, type, onChange, fouten). */}
                    {Object.entries({
                        voornaam: 'Voornaam',
                        achternaam: 'Achternaam',
                        email: 'Email adres',
                        telefoonnummer: 'Telefoonnummer',
                        adress: 'Adres',
                        postcode: 'Postcode',
                        wachtwoord: 'Wachtwoord'
                    }).map(([field, label]) => (
                        <div className="form-group" key={field}>
                            <label htmlFor={field} className="label-text">{label}</label>
                            <div className="input-row">
                                {/* Inputtype en eigenschappen worden conditioneel gekozen (bv. password of email)
                                    en de waarde komt uit `userDetails`. */}
                                <input
                                    disabled={disabledFields[field]}
                                    type={field === 'wachtwoord' ? 'password' : 
                                          field === 'email' ? 'email' :
                                          field === 'telefoonnummer' ? 'tel' : 'text'}
                                    id={field}
                                    name={field}
                                    value={userDetails[field as keyof UserDetails]}
                                    onChange={handleInputChange(field as keyof UserDetails)}
                                    aria-describedby={`${field}-error`}
                                    autoComplete={field === 'wachtwoord' ? 'new-password' : field}
                                />
                                {/* De knop schakelt tussen bewerken en opslaan; toggleField handelt dat af. */}
                                <button
                                    type="button"
                                    className="field-button"
                                    aria-controls={field}
                                    onClick={() => toggleField(field as keyof UserDetails)}
                                >
                                    {disabledFields[field] ? 'Wijzig' : 'Opslaan'}
                                </button>
                            </div>
                            {errors[field] && (
                                <div id={`${field}-error`} className="error-message" aria-live="polite">
                                    {errors[field]}
                                </div>
                            )}
                        </div>
                    ))}

                    <button onClick={handleLogout} className="uitlog-button">
                        Uitloggen
                    </button>
                    <button onClick={handleDeleteAccount} className="verwijder-button">
                        Account verwijderen
                    </button>
                </form>
            </main>
        </div>
    );
};

export default AccountDetails;