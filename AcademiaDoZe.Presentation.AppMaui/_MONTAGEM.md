# Montagem da camada de Apresentação (.NET MAUI) — Atividade 09

Os arquivos desta pasta são o **conteúdo** do app (o que a atividade adiciona por cima do
template). O **projeto MAUI em si precisa ser criado pelo Visual Studio**, porque o template
traz o scaffolding por plataforma (Platforms/), as fontes OpenSans, ícones e splash já
prontos para o seu SDK — coisas que não dá para gerar à mão de forma confiável.

> Por que não veio o projeto inteiro pronto: o workload do MAUI não estava instalado no
> ambiente onde o código foi preparado, então nem o `dotnet new maui` nem o build rodam lá.
> Criar o projeto no VS (passo 1 abaixo) resolve isso de uma vez e é o passo 1 da própria atividade.

## 1. Criar o projeto no Visual Studio
- Botão direito na Solução → **Adicionar → Novo Projeto...**
- Template **.NET MAUI App**, nome **`AcademiaDoZe.Presentation.AppMaui`**, .NET 10.
- **NÃO** marcar "Incluir conteúdo de amostra" nem "Inscrever-se na orquestração do Aspire".
- Botão direito no projeto → **Definir como Projeto de Inicialização**.

## 2. Referências e pacotes
- Botão direito no projeto → **Adicionar → Referência de Projeto** → marcar **AcademiaDoZe.Application**.
- Gerenciador de Pacotes NuGet, instalar no projeto:
  - **CommunityToolkit.Mvvm**
  - **Microsoft.Extensions.DependencyInjection**

## 3. Copiar os arquivos desta pasta para o projeto
Copie, mantendo a estrutura de pastas, para dentro de `AcademiaDoZe.Presentation.AppMaui`:

**Substituir os que o template já criou:**
- `App.xaml.cs`  (corrige o conflito de nome `Application`)
- `AppShell.xaml` e `AppShell.xaml.cs`
- `MauiProgram.cs`

**Adicionar (novos):**
- `Configuration/ConfigurationHelper.cs`
- `ViewModels/BaseViewModel.cs`, `DashboardListViewModel.cs`, `LogradouroListViewModel.cs`, `LogradouroViewModel.cs`
- `Views/DashboardListPage.xaml` (+ `.xaml.cs`), `LogradouroListPage.xaml` (+ `.xaml.cs`), `LogradouroPage.xaml` (+ `.xaml.cs`)

> Dica: se o VS reclamar de arquivo `.xaml` sem `.xaml.cs` "aninhado", basta manter os dois
> com o mesmo nome na mesma pasta que ele reassocia. Se necessário, crie a página pelo próprio
> VS (Add → New Item → .NET MAUI ContentPage (XAML)) e cole o conteúdo.

## 4. Estilos
Abra `Resources/Styles/Styles.xaml` (do template) e cole o conteúdo de
`Resources/Styles/CustomStyles.append.xaml` **antes** do `</ResourceDictionary>` final.
(É só um trecho para colar — não é um arquivo do projeto; pode apagar o `.append.xaml` depois.)

## 5. Logo (obrigatório — a falta ZERA o parcial)
- Gere sua logo contendo **seu nome completo** e salve como
  `Resources/Images/academiadoze.png`. O Dashboard já referencia `academiadoze.png`.
- (É a parte que você mesmo vai fazer; me chame para ajudar a introduzir quando tiver a imagem.)

## 6. Rodar
- Plataforma de destino: **Windows Machine** → F5.
- O banco SQLite precisa existir em `C:\DEV\AcademiaDoZe\db_academia_do_ze.db`
  (o mesmo dos testes). O Dashboard mostra os totais e a tela Logradouros faz o CRUD.
- Para trocar de SGBD, edite `Configuration/ConfigurationHelper.cs` (linha `var databaseType = ...`).

## 7. Entrega no Classroom (recuperação Turma 01: 05/10 18:40) — 4 arquivos
1. Texto/comentário com o **endereço do GIT**.
2. **ZIP da release** criada no GitHub.
3. **Print** da estrutura de diretórios do projeto.
4. **Vídeo** (boa qualidade!) demonstrando: menu, Dashboard com a imagem do seu nome + totais,
   e no Logradouro — cadastrar (com seu nome completo no nome do logradouro), listar/filtrar,
   editar mudando o bairro para **Abc Bolinhas**, e excluir.

> O vídeo, a release e o push para o GitHub são etapas suas (não dá para automatizar aqui).
