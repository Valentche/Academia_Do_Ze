//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Senha
    {
        public string Valor { get; }

        private Senha(string valor)
        {
            Valor = valor;
        }
    }
}
