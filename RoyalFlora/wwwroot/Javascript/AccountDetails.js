function changeDetails(detailsToChange, button) {
  const detail = document.getElementById(detailsToChange);
  
  if (detailsToChange == 'telefoon') {
    detail.innerText = "Werkend?"
  }

  if (detail.disabled) {
    detail.disabled = false;
    button.innerText = "Opslaan";
  } else {
    detail.disabled = true;
    button.innerText = "Gewijzigd";
    setTimeout(() => {
    button.innerText = "Wijzig";
    }, 2000);

    // shit om de data op te slaan
  }
}
