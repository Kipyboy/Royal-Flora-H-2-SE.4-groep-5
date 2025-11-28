import React from "react";
import '../../styles/ProductCard.css';



interface ProductCardProps {
    naam: string;
    merk: string;
    prijs: string;
    datum: string;
    locatie: string;
    status: string;
    Aantal: number;
    FotoPath: string;
}

const ProductCard: React.FC<ProductCardProps> = ({
    naam,
    merk,
    prijs,
    datum,
    locatie,
    status,
    Aantal,
    FotoPath
}) => {
    const defaultImg = "https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg";
    return (
    <div className="product-card">
        <div className="image">
            <img src={FotoPath && FotoPath.trim() !== "" ? FotoPath : defaultImg} alt="" id="product-foto"/>
        </div>
        <div className="info">
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {Aantal}</p>
            <p id="prijs">Prijs: {prijs}</p>
            <p id="datum">Datum: {datum}</p>
            <p id="locatie">Locatie: {locatie}</p>
            <p id="status">Status: {status}</p>
        </div>
    </div>
    );
};

export default ProductCard;