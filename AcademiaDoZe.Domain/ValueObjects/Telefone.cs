//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Telefone
    {
        public string Valor { get; }

        private Telefone(string valor)
        {
            Valor = valor;
        }
    }
}
