# OrderGenerator & OrderAccumulator — FIX 4.4

Sistema de envio e acumulacao de ordens de compra e venda usando o protocolo FIX 4.4.

## Tecnologias

- **.NET 10** — Back-end (Web API, Minimal APIs)
- **QuickFIX/N 1.14** — Protocolo FIX 4.4
- **React 19 + TypeScript** — Front-end (Vite)
- **xUnit + Moq** — Testes unitarios

## Arquitetura

```
[React Frontend] --HTTP--> [OrderGenerator API]
                                  |
                              FIX 4.4 (Initiator)
                                  |
                                  v
                           [OrderAccumulator]
                            FIX 4.4 (Acceptor)
                            Calcula exposicao financeira
```

- **OrderGenerator**: API REST + frontend React. Envia ordens via FIX 4.4 (Initiator).
- **OrderAccumulator**: Recebe ordens via FIX 4.4 (Acceptor), executa e calcula exposicao financeira por simbolo.

## Como executar

### Pre-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)

### 1. Restaurar dependencias

```bash
dotnet restore
cd frontend && npm install && cd ..
```

### 2. Iniciar o OrderAccumulator (primeiro)

```bash
dotnet run --project src/OrderAccumulator
```

O acceptor FIX escuta na porta **5001** (TCP) e a API HTTP na porta **5002**.

### 3. Iniciar o OrderGenerator

```bash
dotnet run --project src/OrderGenerator
```

A API HTTP escuta na porta **5050**. O initiator FIX conecta automaticamente ao acceptor na porta 5001.

### 4. Iniciar o Frontend

```bash
cd frontend
npm run dev
```

Acesse **http://localhost:5173** no navegador.

### 5. Usar

1. Preencha o formulario com Simbolo, Lado, Quantidade e Preco.
2. Clique em "Enviar Ordem".
3. O resultado da execucao sera exibido abaixo do formulario.
4. Para consultar a exposicao financeira acumulada: `GET http://localhost:5002/api/exposure`

## Endpoints

| Metodo | URL | Descricao |
|--------|-----|-----------|
| POST | `http://localhost:5050/api/orders` | Envia uma nova ordem |
| GET | `http://localhost:5002/api/exposure` | Consulta exposicao financeira por simbolo |

## Testes

```bash
dotnet test
```

## Regras de validacao

- **Simbolo**: PETR4, VALE3 ou VIIA4
- **Lado**: Compra (buy) ou Venda (sell)
- **Quantidade**: Inteiro positivo menor que 100.000
- **Preco**: Decimal positivo, multiplo de 0.01, menor que 1.000

## Calculo de exposicao financeira

```
Exposicao[simbolo] = SUM(preco * quantidade) de compras - SUM(preco * quantidade) de vendas
```

---

> This is a challenge by [Coodesh](https://coodesh.com/)
