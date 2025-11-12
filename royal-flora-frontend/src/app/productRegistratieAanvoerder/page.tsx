'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import '../../styles/ProductRegistratieAanvoerder.css';


interface ProductFormData {
	name: string;
	clockLocation: string;
	auctionDate: string;
	amount: string;
	minimumPrice: string;
	description: string;
	images: File[];
}

interface FormErrors {
	name: string;
	clockLocation: string;
	auctionDate: string;
	amount: string;
	minimumPrice: string;
	description: string;
	image: string;
}

export default function ProductRegistratieAanvoerderPage() {
	const router = useRouter();
	const [formData, setFormData] = useState<ProductFormData>({
		name: '',
		clockLocation: '',
		auctionDate: '',
		amount: '',
		minimumPrice: '',
		description: '',
		images: []
	});

	const [errors, setErrors] = useState<FormErrors>({
		name: '',
		clockLocation: '',
		auctionDate: '',
		amount: '',
		minimumPrice: '',
		description: '',
		image: ''
	});

	const [isSubmitting, setIsSubmitting] = useState(false);

	const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		const { name, value } = e.target;
		setFormData(prev => ({
			...prev,
			[name]: value
		}));
		// Error's weg halen waneer je aan het typen bent
		setErrors(prev => ({
			...prev,
			[name]: ''
		}));
	};

	const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		const files = Array.from(e.target.files || []);
		
		// Lijst van de geldige bestands types
		const validFiles: File[] = [];
		const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
		
		for (const file of files) {
			// Geldig bestand?
			if (!validTypes.includes(file.type)) {
				setErrors(prev => ({
					...prev,
					image: 'Alleen afbeeldingen (JPEG, PNG, GIF) zijn toegestaan'
				}));
				continue;
			}
			// Kijken of het niet te groot is
			if (file.size > 5 * 1024 * 1024) {
				setErrors(prev => ({
					...prev,
					image: 'Elk bestand mag maximaal 5MB zijn'
				}));
				continue;
			}
			validFiles.push(file);
		}

		if (validFiles.length > 0) {
			setFormData(prev => ({
				...prev,
				images: [...prev.images, ...validFiles]
			}));
			setErrors(prev => ({
				...prev,
				image: ''
			}));
		}
		
		// File waarde leeg halen zodat er een nieuwe weer gekozen kan worden
		e.target.value = '';
	};

	const removeImage = (index: number) => {
		setFormData(prev => ({
			...prev,
			images: prev.images.filter((_, i) => i !== index)
		}));
	};

	const validateForm = (): boolean => {
		const newErrors: FormErrors = {
			name: '',
			clockLocation: '',
			auctionDate: '',
			amount: '',
			minimumPrice: '',
			description: '',
			image: ''
		};
		let isValid = true;

		// Kijken of een input leeg is of niet
		if (!formData.name.trim()) {
			newErrors.name = 'Product naam is verplicht';
			isValid = false;
		}

		if (!formData.clockLocation) {
			newErrors.clockLocation = 'Klok locatie is verplicht';
			isValid = false;
		}

		if (!formData.auctionDate) {
			newErrors.auctionDate = 'Veilingdatum is verplicht';
			isValid = false;
		} else {
			const selectedDate = new Date(formData.auctionDate);
			const today = new Date();
			today.setHours(0, 0, 0, 0);
			if (selectedDate < today) {
				newErrors.auctionDate = 'Veilingdatum moet in de toekomst liggen';
				isValid = false;
			}
		}

		if (!formData.amount) {
			newErrors.amount = 'Aantal is verplicht';
			isValid = false;
		} else if (parseInt(formData.amount) <= 0) {
			newErrors.amount = 'Aantal moet groter dan 0 zijn';
			isValid = false;
		}

		if (!formData.minimumPrice) {
			newErrors.minimumPrice = 'Minimum prijs is verplicht';
			isValid = false;
		} else if (parseFloat(formData.minimumPrice) < 0) {
			newErrors.minimumPrice = 'Minimum prijs moet positief zijn';
			isValid = false;
		}

		if (!formData.description.trim()) {
			newErrors.description = 'Omschrijving is verplicht';
			isValid = false;
		} else if (formData.description.trim().length < 10) {
			newErrors.description = 'Omschrijving moet minimaal 10 karakters zijn';
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

		setIsSubmitting(true);

		try {
			// data opslaan om naar de database te stuuren
			const submitData = new FormData();
			submitData.append('productNaam', formData.name);
			submitData.append('productBeschrijving', formData.description);
			submitData.append('minimumPrijs', formData.minimumPrice);
			submitData.append('clockLocation', formData.clockLocation);
			submitData.append('auctionDate', formData.auctionDate);
			submitData.append('amount', formData.amount);
			
			formData.images.forEach((image, index) => {
				submitData.append(`images[${index}]`, image);
			});

			// naar de database stuuren
			const response = await fetch('http://localhost:5156/api/product/register', {
				method: 'POST',
				body: submitData,
				// Ik werd gewaarschuwed dat ik geen header type moet mee geven
			});

			if (response.ok) {
				const data = await response.json();
				alert('Product succesvol geregistreerd!');
				// Alles leeg halen
				setFormData({
					name: '',
					clockLocation: '',
					auctionDate: '',
					amount: '',
					minimumPrice: '',
					description: '',
					images: []
				});
				const fileInput = document.getElementById('image') as HTMLInputElement;
				if (fileInput) fileInput.value = '';

			} else {
				const error = await response.json();
				alert(`Registratie mislukt: ${error.message || 'Onbekende fout'}`);
			}
		} catch (error) {
			console.error('Error:', error);
			alert('Er is een fout opgetreden bij het registreren van het product');
		} finally {
			setIsSubmitting(false);
		}
	};

	return (
		<div className="productRegistratieAanvoerder-page">
			<nav>
				<span className="nav-text">Product registreren aanvoerder</span>
				<img
					src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Royal_FloraHolland_Logo.svg/1200px-Royal_FloraHolland_Logo.svg.png"
					alt="Royal FloraHolland Logo"
				/>
				<Link className="pfp-container" href="/accountDetails">
					<img
						src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
						alt="Profiel foto"
					/>
				</Link>
			</nav>

			<div className="content">
				<form className="formContainer" onSubmit={handleSubmit}>
					<div className="groupContainer">
						<label htmlFor="name">Product naam:</label>
						<input 
							id="name" 
							name="name" 
							type="text"
							value={formData.name}
							onChange={handleInputChange}
							aria-describedby="name-error"
							required
						/>
						{errors.name && (
							<div id="name-error" className="error-message" aria-live="polite">
								{errors.name}
							</div>
						)}
					</div>

					<div className="inlineGroup">
						<div className="groupContainer">
							<label htmlFor="clockLocation">Klok locatie:</label>
							<input 
								id="clockLocation" 
								name="clockLocation" 
								type="datetime-local"
								value={formData.clockLocation}
								onChange={handleInputChange}
								aria-describedby="clockLocation-error"
								required
							/>
							{errors.clockLocation && (
								<div id="clockLocation-error" className="error-message" aria-live="polite">
									{errors.clockLocation}
								</div>
							)}
						</div>

						<div className="groupContainer">
							<label htmlFor="auctionDate">Veilingdatum:</label>
							<input 
								id="auctionDate" 
								name="auctionDate" 
								type="date"
								value={formData.auctionDate}
								onChange={handleInputChange}
								aria-describedby="auctionDate-error"
								required
							/>
							{errors.auctionDate && (
								<div id="auctionDate-error" className="error-message" aria-live="polite">
									{errors.auctionDate}
								</div>
							)}
						</div>
					</div>

					<div className="inlineGroup">
						<div className="groupContainer">
							<label htmlFor="amount">Aantal:</label>
							<input 
								id="amount" 
								name="amount" 
								type="number"
								min="1"
								value={formData.amount}
								onChange={handleInputChange}
								aria-describedby="amount-error"
								required
							/>
							{errors.amount && (
								<div id="amount-error" className="error-message" aria-live="polite">
									{errors.amount}
								</div>
							)}
						</div>

						<div className="groupContainer">
							<label htmlFor="minimumPrice">Minimum prijs (€):</label>
							<input 
								id="minimumPrice" 
								name="minimumPrice" 
								type="number"
								min="0"
								step="0.01"
								value={formData.minimumPrice}
								onChange={handleInputChange}
								aria-describedby="minimumPrice-error"
								required
							/>
							{errors.minimumPrice && (
								<div id="minimumPrice-error" className="error-message" aria-live="polite">
									{errors.minimumPrice}
								</div>
							)}
						</div>
					</div>

					<div className="groupContainer">
						<label htmlFor="description">Omschrijving:</label>
						<input 
							id="description" 
							name="description" 
							type="text" 
							className="bigInput"
							value={formData.description}
							onChange={handleInputChange}
							aria-describedby="description-error"
							required
							minLength={10}
						/>
						{errors.description && (
							<div id="description-error" className="error-message" aria-live="polite">
								{errors.description}
							</div>
						)}
					</div>

					<div className="groupContainer">
						<label htmlFor="image">Upload afbeelding(en):</label>
						<input 
							id="image" 
							name="image" 
							type="file"
							accept="image/jpeg,image/jpg,image/png,image/gif"
							onChange={handleFileChange}
							aria-describedby="image-error"
							multiple
						/>
						{formData.images.length > 0 && (
							<div className="image-preview-container">
								{formData.images.map((img, index) => (
									<div key={index} className="image-preview-item">
										<img 
											src={URL.createObjectURL(img)} 
											alt={`Preview ${index + 1}`}
											className="image-preview"
										/>
										<div className="image-preview-info">
											<span className="image-name">{img.name}</span>
											<button 
												type="button"
												className="remove-image-btn"
												onClick={() => removeImage(index)}
												aria-label={`Verwijder ${img.name}`}
											>
												✕
											</button>
										</div>
									</div>
								))}
							</div>
						)}
						{errors.image && (
							<div id="image-error" className="error-message" aria-live="polite">
								{errors.image}
							</div>
						)}
					</div>

					<div className="groupContainer">
						<input 
							type="submit" 
							className="submitButton" 
							value={isSubmitting ? 'Registreren...' : 'Registreer Product'}
							disabled={isSubmitting}
						/>
					</div>
				</form>
			</div>
		</div>
	);
}
