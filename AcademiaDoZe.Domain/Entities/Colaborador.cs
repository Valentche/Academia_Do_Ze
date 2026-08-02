//Pablo Valente Neto

using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class Colaborador : Pessoa
    {
        public DateOnly DataAdmissao { get; private set; }
        public ColaboradorTipo Tipo { get; private set; }
        public ColaboradorVinculo Vinculo { get; private set; }

        private Colaborador(
            int id,
            string nome,
            Cpf cpf,
            DateOnly dataNascimento,
            Telefone telefone,
            Email? email,
            Endereco endereco,
            Senha senha,
            Arquivo? foto,
            DateOnly dataAdmissao,
            ColaboradorTipo tipo,
            ColaboradorVinculo vinculo)
            : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
        {
            DataAdmissao = dataAdmissao;
            Tipo = tipo;
            Vinculo = vinculo;
        }
    }
}