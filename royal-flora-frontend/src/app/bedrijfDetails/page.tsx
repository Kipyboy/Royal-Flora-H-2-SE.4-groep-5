
"use client"
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/AccountDetails.css';
import Topbar from '../components/Topbar';
import { getAuthHeaders } from '../utils/auth';
import { API_BASE_URL } from '../config/api';

// Deze pagina toont bedrijfsgegevens. Gebruik consistente, backend-matching keys.

interface BedrijfInfo {
    bedrijfNaam: string;
    postcode: string;
    adress: string;
    oprichter: string;
}

const BedrijfDetails: React.FC = () => {
    const router = useRouter();
    const [bedrijfDetails, setBedrijfDetails] = useState<BedrijfInfo>({
        bedrijfNaam: '',
        postcode: '',
        adress: '',
        oprichter: '',
    });
    const [isOprichter, setIsOprichter] = useState<boolean>(false);

    const [disabledFields, setDisabledFields] = useState<Record<keyof BedrijfInfo, boolean>>({
        bedrijfNaam: true,
        postcode: true,
        adress: true,
        oprichter: true
    });

    const [errors, setErrors] = useState<Record<keyof BedrijfInfo, string>>({
        bedrijfNaam: '',
        postcode: '',
        adress: '',
        oprichter: ''
    });
    const [loading, setLoading] = useState<boolean>(true);

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

                const response = await fetch(`${API_BASE_URL}/api/auth/GetBedrijfInfo`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...authHeaders,
                    }
                });

                if (response.ok) {
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
                setErrors(prev => ({ ...prev, bedrijfNaam: 'Kon sessie gegevens niet ophalen' }));
            } finally {
                setLoading(false);
            }
        };

        fetchSessionData();
    }, []);


    const handleInputChange = (field: keyof BedrijfInfo) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setBedrijfDetails(prev => ({
            ...prev,
            [field]: e.target.value
        }));
    };

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

    const handleSave = async (field: keyof BedrijfInfo) => {
        try {
            console.log(`Saving field ${field} with value ${bedrijfDetails[field]}`);
            const authHeaders = getAuthHeaders();

            if (!authHeaders.Authorization) {
                router.push('/');
                return;
            }

            // data opslag hier
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


