//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Cep
    {
        public string Valor { get; }

        private Cep(string valor)
        {
            Valor = valor;
        }
    }
}
