
"use client"
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/AccountDetails.css';
import Topbar from '../components/Topbar';
import { getAuthHeaders } from '../utils/auth';
import { API_BASE_URL } from '../config/api';

// Deze pagina toont bedrijfsgegevens. Gebruik consistente, backend-matching keys.
// `getAuthHeaders` haalt de Authorization header op (JWT of soortgelijk token) die nodig is
// voor beveiligde API-aanroepen. `API_BASE_URL` is de basis-URL voor de backend API.


// `BedrijfInfo` beschrijft de bedrijfsgegevens die we tonen en (deels) kunnen bewerken.
interface BedrijfInfo {
    bedrijfNaam: string;
    postcode: string;
    adress: string;
    oprichter: string;
} 

const BedrijfDetails: React.FC = () => {
    // `router` gebruiken voor navigatie (redirects bij ontbrekende token of fouten)
    const router = useRouter();

    // `bedrijfDetails` bevat de huidige waarden van het formulier
    const [bedrijfDetails, setBedrijfDetails] = useState<BedrijfInfo>({
        bedrijfNaam: '',
        postcode: '',
        adress: '',
        oprichter: '',
    });
    // `isOprichter` bepaalt of de ingelogde gebruiker oprichterrechten heeft (en dus sommige knoppen ziet)
    const [isOprichter, setIsOprichter] = useState<boolean>(false);

    // `disabledFields` regelt welke velden readonly zijn (true = disabled)
    const [disabledFields, setDisabledFields] = useState<Record<keyof BedrijfInfo, boolean>>({
        bedrijfNaam: true,
        postcode: true,
        adress: true,
        oprichter: true
    });

    // `errors` houdt foutmeldingen per veld bij
    const [errors, setErrors] = useState<Record<keyof BedrijfInfo, string>>({
        bedrijfNaam: '',
        postcode: '',
        adress: '',
        oprichter: ''
    });
    // `loading` boolean om de initiële laadstatus te tonen
    const [loading, setLoading] = useState<boolean>(true); 

    // useEffect: bij mount de bedrijfsinformatie ophalen van de backend
    React.useEffect(() => {
        const fetchSessionData = async () => {
            try {
                const authHeaders = getAuthHeaders();

                // Check if we have a token
                if (!authHeaders.Authorization) {
                    console.error('No token found');
                    router.push('/');
                    return;
                }

                // Ophalen van bedrijfgegevens (protected endpoint)
                const response = await fetch(`${API_BASE_URL}/api/auth/GetBedrijfInfo`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...authHeaders,
                    }
                });

                if (response.ok) {
                    // Zet ontvangen data in state en bewaar of de gebruiker oprichter is
                    const data = await response.json();
                    setBedrijfDetails({
                        bedrijfNaam: data.bedrijfNaam || '',
                        postcode: data.postcode || '',
                        adress: data.adres || '',
                        oprichter: data.oprichter || ''
                    });
                    setIsOprichter(!!data.isOprichter);
                } else {
                    console.error('Failed to fetch session data');
                    router.push('/');
                }
            } catch (error) {
                console.error('Error fetching session:', error);
                // Toon foutmelding bij het juiste veld
                setErrors(prev => ({ ...prev, bedrijfNaam: 'Kon sessie gegevens niet ophalen' }));
            } finally {
                // Stop laadindicator
                setLoading(false);
            }
        };

        fetchSessionData();
    }, []); 


    // Retourneert een onChange handler voor een gegeven veld zodat inputs state updaten
    const handleInputChange = (field: keyof BedrijfInfo) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setBedrijfDetails(prev => ({
            ...prev,
            [field]: e.target.value
        }));
    }; 

    // Toggle: maak velden bewerkbaar of sla ze op als ze al bewerkbaar zijn.
    // Speciale behandeling: 'oprichter' mag niet via deze UI aangepast worden.
    const toggleField = (field: keyof BedrijfInfo) => {
        if (field == 'oprichter') {
            setErrors(prev => ({
                ...prev,
                [field]: 'Kan oprichter niet aanpassen, neem hiervoor contact op met jem-id'
        }))
            return;
        }
        if (disabledFields[field]) {
            setDisabledFields(prev => ({
                ...prev,
                [field]: false
            }));
        } else {
            handleSave(field);
        }
    }; 

    // Slaat een enkel bedrijfsveld op via de backend. Controleert token en behandelt fouten.
    const handleSave = async (field: keyof BedrijfInfo) => {
        try {
            const authHeaders = getAuthHeaders();

            if (!authHeaders.Authorization) {
                router.push('/');
                return;
            }

            // Verstuur update naar backend (Field/ Newvalue) - backend bepaalt verdere validatie
            const response = await fetch(`${API_BASE_URL}/api/auth/updateBedrijfInfo`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...authHeaders
                },
                body: JSON.stringify({
                    Field: field,
                    Newvalue: bedrijfDetails[field]
                })
            });

            if (response.ok) {
                // Bij succes: maak veld weer readonly en clear eventuele foutmelding
                setDisabledFields(prev => ({
                    ...prev,
                    [field]: true
                }));

                setErrors(prev => ({
                    ...prev,
                    [field]: ''
                }));
            } else {
                setErrors(prev => ({
                    ...prev,
                    [field]: 'Er ging iets mis bij het opslaan'
                }));
            }
        } catch (error) {
            setErrors(prev => ({
                ...prev,
                [field]: 'Er ging iets mis bij het opslaan'
            }));
        }
    }; 

    // Toon laadindicator totdat bedrijfsgegevens zijn opgehaald
    if (loading) {
        return (
            <div className='accountDetails-page'>
                <Topbar
                    currentPage="Bedrijf Details"
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
                currentPage="Bedrijf Details"
                useSideBar={false}
            />

            <main id="main">
                <form onSubmit={e => e.preventDefault()} aria-labelledby="bedrijfdetails-title">
                    <h1 id="bedrijfdetails-title">Bedrijf details</h1>

                    {/* Dynamisch genereren van form-velden: sleutel -> label. Dit zorgt voor consistente logica
                        bij elk veld (disabled state, type, onChange en foutmeldingen). */}
                    {Object.entries({
                        bedrijfNaam: 'Bedrijfsnaam',
                        postcode: 'Postcode',
                        adress: 'Adres',
                        oprichter: 'Oprichter'
                    } as Record<keyof BedrijfInfo, string>).map(([field, label]) => (
                        <div className="form-group" key={field}>
                            <label htmlFor={field} className="label-text">{label}</label>
                            <div className="input-row">
                                <input
                                    disabled={disabledFields[field as keyof BedrijfInfo]}
                                    type={field === 'wachtwoord' ? 'password' : 'text'}
                                    id={field}
                                    name={field}
                                    value={bedrijfDetails[field as keyof BedrijfInfo]}
                                    onChange={handleInputChange(field as keyof BedrijfInfo)}
                                    aria-describedby={`${field}-error`}
                                    autoComplete={field === 'wachtwoord' ? 'new-password' : field}
                                />
                                <button
                                    type="button"
                                    className="field-button"
                                    aria-controls={field}
                                    onClick={() => toggleField(field as keyof BedrijfInfo)}
                                    style={{ display: isOprichter ? 'block' : 'none' }}
                                >
                                    {disabledFields[field as keyof BedrijfInfo] ? 'Wijzig' : 'Opslaan'}
                                </button>
                            </div>
                            {errors[field as keyof BedrijfInfo] && (
                                <div id={`${field}-error`} className="error-message" aria-live="polite">
                                    {errors[field as keyof BedrijfInfo]}
                                </div>
                            )}
                        </div>
                    ))}

                </form>
            </main>
        </div>
    );
};

export default BedrijfDetails;


