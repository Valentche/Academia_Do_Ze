//Pablo Valente Neto

using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class Aluno : Pessoa
    {
        private Aluno(
            int id,
            string nome,
            Cpf cpf,
            DateOnly dataNascimento,
            Telefone telefone,
            Email? email,
            Endereco endereco,
            Senha senha,
            Arquivo? foto)
            : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
        {
        }
    }
}
