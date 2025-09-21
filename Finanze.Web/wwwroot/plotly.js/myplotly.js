
////salva gli id dei grafici per la possibilita di aggiornare graficamente la dimensione del grafico
let ids = [];

window.generateGenericChart = function (idchart, data, layout, config) {

    const element = document.getElementById(idchart);

    if (!element) {
        return; // esci dalla funzione se il div non esiste
    }

    const screenWidth = window.innerWidth;

    // Controllo se layout esiste e se showlegend è true
    if (layout && layout.showlegend === true && screenWidth < 768) {
        layout.showlegend = false; // Disattiva la legenda su schermi piccoli
    }

    ids.push(idchart);
    Plotly.newPlot(document.getElementById(idchart), data, layout, config);
};

/**
* Metodo per aggiornare i grafici nel momento in cui la sidebar viene aperta o chiusa 
* Tutti i metodi che generano un chart, vanno a salvare l'id dell'elemento dentro un array
*/
window.updateChartDimension = function () {
    // Rimuove i duplicati con Set
    const distinctIds = [...new Set(ids)];

    setTimeout(() => {
        // Ciclo sull'array senza duplicati
        distinctIds.forEach(id => {
            try {
                const element = document.getElementById(id);
                if (element) {
                    Plotly.Plots.resize(document.getElementById(id));
                }

            } catch (e) {
                console.log(e)
            }
        });
    }, 100);
}


window.purgeChart = function (idChart) {

    Plotly.purge(idChart); // libera la memoria usata da Plotly
    document.getElementById(idChart).innerHTML = '';

}