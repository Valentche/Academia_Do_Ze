//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Arquivo
    {
        public string Nome { get; }
        public byte[] Conteudo { get; }

        private Arquivo(string nome, byte[] conteudo)
        {
            Nome = nome;
            Conteudo = conteudo;
        }
    }
}
