# WpfApp

Aplicação desktop em C# com WPF (.NET Framework 4.6) para cadastro de Pessoas, Produtos e Pedidos.
Com persistência em arquivos (JSON) e uso de LINQ, seguindo padrão MVVM.

Status: Em construção

## Requisitos
- Visual Studio 2019+ (ou 2022) com suporte a .NET Framework
- .NET Framework 4.6
- NuGet: Newtonsoft.Json

## Como executar
1. Clone este repositório
2. Abra a solução `WpfApp.sln` no Visual Studio
3. Restaure os pacotes NuGet
4. Execute (F5)

## Estrutura
WpfApp/
├── Models/ # Classes de domínio (Pessoa, Produto, Pedido)
├── Views/ # Telas WPF (XAML)
├── ViewModels/ # Lógica de apresentação (MVVM)
├── Services/ # Serviços de persistência e lógica de negócio
├── Data/ # Arquivos JSON/XML
├── Resources/ # Ícones, imagens, etc.
├── Helpers/ # Helpers e utilitários
│ └── Infra/ # Infraestrutura (ex: RelayCommand)
└── README.md # Instruções do projeto