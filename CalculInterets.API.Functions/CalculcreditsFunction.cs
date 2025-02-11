using CalculInterets.API.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CalculInterets.API.Functions
{
    public class CalculcreditsFunction
    {
        private readonly ILogger<CalculcreditsFunction> _logger;

        public CalculcreditsFunction(ILogger<CalculcreditsFunction> logger)
        {
            _logger = logger;
        }


        [Function("calculCredit")]
        public async Task<InteretsOutput> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {

            _logger.LogInformation("Azure function pour le calcul des intérets");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            List<Interet> interets = JsonConvert.DeserializeObject<List<Interet>>(requestBody);

            foreach (var interet in interets)
            {

                int nombreMois = ((interet.DateFin.Year - interet.DateDebut.Year) * 12) + interet.DateFin.Month - interet.DateDebut.Month;

                double tauxMensuel = (interet.Taux / 100) / 12;
                double montantFinal = interet.Solde * Math.Pow(1 + tauxMensuel, nombreMois);

                interet.MontantInteret = Math.Round(montantFinal - interet.Solde, 2);
            }

            return new InteretsOutput() { 
                Interets = interets
            } ;
        }
    }

    public  class InteretsOutput
    {
        [SqlOutput("dbo.Interet", connectionStringSetting: "SqlConnectionString")]
        public List<Interet> Interets { get; set; }
    }
}
