window.addEventListener("DOMContentLoaded", () => {
  const sections = document.querySelectorAll(".input-row");

  sections.forEach(section => {
    const inputs = section.querySelectorAll("input");
    inputs.forEach(input => input.disabled = true);
  });
});




function changeDetails(detailsToChange, button) {
  const detail = document.getElementById(detailsToChange);
  
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
