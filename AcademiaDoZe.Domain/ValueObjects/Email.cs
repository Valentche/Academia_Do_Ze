//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Email
    {
        public string Valor { get; }

        private Email(string valor)
        {
            Valor = valor;
        }
    }
}
