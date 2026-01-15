import React, { useState } from "react";
import '../../styles/ProductCard.css';
import { API_BASE_URL } from '../config/api';
import { authFetch } from "../utils/api";


interface VeilingmeesterProductCardProps {
    id: number;
    naam: string;
    merk: string;
    prijs: string | number;
    datum: string;
    locatie: string;
    status: string;
    aantal: number;
    fotoPath: string;
    beschrijving: string;
    toonBeschrijving: boolean;
}

// Verwijdert een product via de API. Geeft feedback bij succes/fout.
const handleDelete = async (id: number) => {
    try{
        const response = await authFetch(
            `${API_BASE_URL}/api/Products/${id}`,
            {
                method: "DELETE",
            }
        );
        
        if (response.ok) {
            // TODO: verbeter: refresh lijst of verwijder item uit parent state in plaats van alleen alert
            alert("Product deleted successfully");
        }
    }
    catch(error){
        alert("Error deleting product");
    }
    return;
} 


const VeilingmeesterProductCard: React.FC<VeilingmeesterProductCardProps> = ({
    id,
    naam,
    merk,
    prijs,
    datum,
    locatie,
    status,
    aantal,
    fotoPath
    , beschrijving,
    toonBeschrijving
}) => {
    
    const defaultImg = "https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg";
    return (
    <div className="product-card">
        <div className="image">
            <img src={fotoPath && fotoPath.trim() !== "" ? `${API_BASE_URL}/images/${fotoPath}` : defaultImg} alt="" id="product-foto"/>
        </div>
        <div className="info" style={{ display : toonBeschrijving ? 'none' : 'block'}}>
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {aantal}</p>
            <p id="prijs">Prijs: {prijs}</p>
            <p id="datum">Datum: {datum}</p>
            <p id="locatie">Locatie: {locatie}</p>
            <p id="status">Status: {status}</p>
        </div>
        <div className="beschrijving" style={{ display: toonBeschrijving ? 'block' : 'none' }}>
            {beschrijving}
        </div>
        <button className="delete-button" onClick={() => handleDelete(id)}>Delete</button>
    </div>
    );
};

export default VeilingmeesterProductCard;