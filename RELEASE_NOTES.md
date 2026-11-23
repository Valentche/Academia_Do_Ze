# 🚀 Academia do Zé — Release Notes

---

## 🏆 Versão: `v1.5.0-avaliacao03`
**Data:** 23 de Novembro de 2026 &nbsp;|&nbsp; **Branch:** `avaliacao03` / `master` &nbsp;|&nbsp; **Tag:** `v1.5.0-avaliacao03`

### 🌟 Visão Geral da Versão
Entrega oficial da **Avaliação 03 – Prova Prática**, implementando o **Módulo de Controle de Acesso / Catraca** para entrada e saída de alunos e colaboradores, totalmente integrado ao banco de dados e seguindo os padrões de Clean Architecture e MVVM no .NET MAUI.

```
┌────────────────────────────────────────────────────────────────────────┐
│                        ACADEMIA DO ZÉ • v1.5.0                         │
│  🚪 Controle de Acesso / Catraca  •  🎓 Alunos  •  💼 Colaboradores   │
└────────────────────────────────────────────────────────────────────────┘
```

### ✨ Funcionalidades da Avaliação 03

#### 1. 🎓 Controle de Acesso de Alunos
* **Validação de Matrícula Ativa:**
  * Aluno **sem matrícula ativa** $\rightarrow$ **Acesso Negado** com bloqueio e alerta informativo na tela.
  * Aluno **com matrícula ativa** $\rightarrow$ **Entrada Autorizada** com registro de data/hora no banco `tb_acesso`.
* **Registro de Saída:**
  * Saída do mesmo aluno registrada com sucesso ao passar novamente pela catraca, alternando o status para `FORA DA ACADEMIA`.

#### 2. 💼 Controle de Acesso de Colaboradores
* **Registro de Ponto / Entrada:**
  * Entrada registrada com data e hora.
* **Registro de Ponto / Saída:**
  * Saída registrada com persistência no banco e histórico do dia.

#### 3. 🖥️ Interface de Controle de Acesso (`ControleAcessoPage`)
* **Alternador de Modo:** Abas intuitivas `[ 🎓 Aluno ]` e `[ 💼 Colaborador ]`.
* **Localização Rápida:** Campo de busca em tempo real com auto-seleção e Picker.
* **Identificação e Liberação:** Foto, Nome completo, CPF, Badge de status (`DENTRO DA ACADEMIA` / `FORA DA ACADEMIA`) e botão de ação dinâmico (`Registrar Entrada` / `Registrar Saída`).
* **Banner de Feedback Colorido:** Notificação visual clara e imediata do status da catraca (*Entrada Autorizada*, *Saída Registrada* ou *Acesso Negado*).
* **Histórico do Dia:** Lista reativa com todos os acessos de hoje e botão para atualização manual.

#### 4. 🎨 Identidade Visual & Requisito Eliminatório
* Imagem da logo na Dashboard com assinatura autoral **`"by Pablo Valente"`**, atendendo plenamente à exigência do parcial.

---

## 📋 Versão Anterior: `v1.4.0-aula14-15`
**Data:** 08 de Novembro de 2026 &nbsp;|&nbsp; **Branch:** `aula14-15` / `master`

* **Módulo de Matrículas (CRUD Completo):** Vigência automatizada, planos, restrições médicas em flags bitwise e laudo médico digital.
* **Busca Inteligente:** Filtro de matrículas por nome/CPF e exibição destacada do ID nos cards (`[#ID]`).
* **Easter Egg Secreto:** Código `"deteuspulos"` nas configurações com vídeo streaming local.
