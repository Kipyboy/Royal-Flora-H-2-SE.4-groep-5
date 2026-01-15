import React, { useState } from "react";
import '../../styles/ProductCard.css';
import { API_BASE_URL } from '../config/api';



// Kaartcomponent voor een gekochte productvermelding.
// Toont basisinformatie (naam, merk, prijs, datum, locatie, status) en kan optioneel
// een uitgebreide beschrijving laten zien wanneer `toonBeschrijving` true is.
interface GekochtProductCardProps {
    naam: string;
    merk: string;
    verkoopPrijs: string;
    datum: string;
    locatie: string;
    status: string;
    aantal: number;
    fotoPath: string;
    beschrijving: string;
    toonBeschrijving: boolean;
}

const GekochtProductCard: React.FC<GekochtProductCardProps> = ({
    naam,
    merk,
    verkoopPrijs,
    datum,
    locatie,
    status,
    aantal,
    fotoPath
    , beschrijving,
    toonBeschrijving
}) => {
    
    // Fallback-afbeelding wanneer er geen `fotoPath` beschikbaar is
    const defaultImg = "https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg";
    return (
    <div className="product-card">
        <div className="image">
            <img src={fotoPath && fotoPath.trim() !== "" ? `${API_BASE_URL}/images/${fotoPath}` : defaultImg} alt="" id="product-foto"/>
        </div>
        <div className="info" style={{ display: toonBeschrijving ? 'none' : 'block'}}>
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {aantal}</p>
            <p id="prijs">Gekocht voor: {verkoopPrijs}</p>
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

export default GekochtProductCard;