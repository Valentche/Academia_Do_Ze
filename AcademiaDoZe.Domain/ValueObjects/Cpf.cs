//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Cpf
    {
        public string Valor { get; }
        private Cpf(string valor)
        {
            Valor = valor;
        }
    }
}
