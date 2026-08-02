//Pablo Valente Neto

using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class Matricula : Entity
    {
        public Aluno Aluno { get; private set; }
        public MatriculaPlano Plano { get; private set; }
        public DateOnly DataInicio { get; private set; }
        public DateOnly DataFinal { get; private set; }
        public string Objetivo { get; private set; }
        public MatriculaRestricoes Restricoes { get; private set; }
        public string? ObservacoesRestricoes { get; private set; }
        public Arquivo? LaudoMedico { get; private set; }

        private Matricula(
            int id,
            Aluno aluno,
            MatriculaPlano plano,
            DateOnly dataInicio,
            DateOnly dataFinal,
            string objetivo,
            MatriculaRestricoes restricoes,
            string? observacoesRestricoes,
            Arquivo? laudoMedico)
            : base(id)
        {
            Aluno = aluno;
            Plano = plano;
            DataInicio = dataInicio;
            DataFinal = dataFinal;
            Objetivo = objetivo;
            Restricoes = restricoes;
            ObservacoesRestricoes = observacoesRestricoes;
            LaudoMedico = laudoMedico;
        }
    }
}
