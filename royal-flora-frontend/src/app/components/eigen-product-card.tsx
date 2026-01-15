import React, { useState } from "react";
import '../../styles/ProductCard.css';
import { API_BASE_URL } from '../config/api';



// Kaartcomponent voor een eigen product (door de gebruiker geregistreerd).
// Toont informatie inclusief koper/prijs als beschikbaar en ondersteunt het tonen
// van een langere `beschrijving` wanneer `toonBeschrijving` true is.
interface EigenProductCardProps {
    naam: string;
    merk: string;
    verkoopPrijs: string;
    koper: string;
    datum: string;
    locatie: string;
    status: string;
    aantal: number;
    fotoPath: string;
    beschrijving: string;
    toonBeschrijving: boolean;
}

const EigenProductCard: React.FC<EigenProductCardProps> = ({
    naam,
    merk,
    verkoopPrijs,
    koper,
    datum,
    locatie,
    status,
    aantal,
    fotoPath,
    beschrijving,
    toonBeschrijving
}) => {
    
    // Fallback-afbeelding wanneer product geen afbeelding heeft
    const defaultImg = "https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg";
    
    return (
    <div className="product-card">
        
        <div className="image">
            <img src={fotoPath && fotoPath.trim() !== "" ? `${API_BASE_URL}/images/${fotoPath}` : defaultImg} alt="" id="product-foto"/>
        </div>
        <div className="info" style={{display : toonBeschrijving ? 'none' : 'block'}}>
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {aantal}</p>
            <p id="prijs">Verkoopprijs: {koper.trim() == "" || koper == null ? "Nog niet verkocht" : verkoopPrijs}</p>
            <p id="koper">Koper: {koper.trim() == "" || koper == null ? "Nog niet verkocht" : koper}</p>
            <p id="datum">Datum: {datum}</p>
            <p id="locatie">Locatie: {locatie}</p>
            <p id="status">Status: {status}</p>
        </div>
        <div className="beschrijving" style={{ display: toonBeschrijving ? 'block' : 'none' }}>
            {beschrijving}
        </div>
    </div>
    );
};

export default EigenProductCard;