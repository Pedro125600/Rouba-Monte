using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoubaMonte
{
    public class Carta
    {
        public int Numero { get; set; }
        public string Naipe { get; set; }

        public Carta(int numero, string naipe)
        {
            Numero = numero;
            Naipe = naipe;
        }

        public string DescreverCarta() 
        {
            if (Numero == 1)
            {
                return $"Ás de {Naipe}";
            }
            else if (Numero > 1 && Numero <= 10)
            {
                return $"{Numero} de {Naipe}";
            }
            else if (Numero == 11)
            {
                return $"Valete de {Naipe}";
            }
            else if (Numero == 12)
            {
                return $"Dama de {Naipe}";
            }
            else if (Numero == 13)
            {
                return $"Rei de {Naipe}";
            }
            else
            {
                return $"Carta inválida ({Numero} de {Naipe})";
            }
        }


    }

    public class Jogador
    {
        public string Nome { get; set; }
        public int Posicao { get; set; }
        public int QuantidadeCartas;

      
        public Fila rankingUltimas5Partidas { get; set; } 

        public void AdicionarRanking()
        {
            if (rankingUltimas5Partidas.Tamanho() == 5)
            {
                rankingUltimas5Partidas.Remover(); 
            }
            rankingUltimas5Partidas.Inserir(Posicao);
        }

        public void MostrarRanking()
        {
            Console.WriteLine("Ranking das últimas 5 partidas:");
            rankingUltimas5Partidas.Mostrar();
        }

        public Stack<Carta> Monte { get; private set; } = new Stack<Carta>();


        public void AtualizarQuantidadeDeCartas() 
        {
            this.QuantidadeCartas = Monte.Count;
        }

        public Jogador(string nome)
        {
            Nome = nome;
            rankingUltimas5Partidas = new Fila();
        }



        public string DescreverJogador() 
        {
            return Posicao + "º lugar: " + Nome + " com " + QuantidadeCartas + " cartas.";
        }

    }

    public class Jogo
    {
        private List<Jogador> jogadores = new List<Jogador>();
        private Pilha monteCompra;
        private List<Carta> areaDescarte = new List<Carta>();
        private int quantidadeBaralhos;
        private int quantidadeJogadores;
        private string logFilePath = "log_partida.txt";


        public Jogo(List<Jogador> jogadores, int quantidadeBaralhos)
        {
            this.jogadores = jogadores;
            this.quantidadeBaralhos = quantidadeBaralhos;
            int tamanhoMonte = quantidadeBaralhos * 52; 
            monteCompra = new Pilha(tamanhoMonte);
        }



        private void GerarBaralho()
        {


            for (int i = 0; i < quantidadeBaralhos; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    string[] naipes = { "Copas", "Espadas", "Ouros", "Paus" };
                    for (int k = 0; k < naipes.Length; k++)
                    {
                        monteCompra.Push(new Carta(j, naipes[k]));
                    }
                }
            }
        }

        private void EmbaralharCartas()
        {
            List<Carta> temp = new List<Carta>();

            
            while (monteCompra.topo > 0)
            {
                temp.Add(monteCompra.Pop());
            }

            Random randomizarCartas = new Random();
            while (temp.Count > 0)
            {
                int index = randomizarCartas.Next(temp.Count);
                monteCompra.Push(temp[index]);
                temp.RemoveAt(index);
            }
        }

        public void LimparMontesDosJogadores()
        {
            foreach (var jogador in jogadores)
            {
                jogador.Monte.Clear(); 
            }
            Console.WriteLine("Montes dos jogadores foram limpos.");
        }

        public void Iniciar()
        {

            LimparMontesDosJogadores();

            GerarBaralho();
            EmbaralharCartas();

            JogarPartida();

        }

        private void ExibirEstadoAtual(Jogador jogador, Carta cartaDaVez)
        {
            Console.Clear();
            Console.WriteLine($"Jogador atual: {jogador.Nome}");

            Console.Write($"Monte de {jogador.Nome}: ");
            foreach (var carta in jogador.Monte)
            {
                Console.Write($"{carta.DescreverCarta()}, ");
            }
            Console.WriteLine();

            Console.WriteLine($"Carta da vez: {cartaDaVez.DescreverCarta()}");

            Console.Write("Área de descarte: ");
            foreach (var carta in areaDescarte)
            {
                Console.Write($"{carta.DescreverCarta()}, ");
            }

            Console.WriteLine();
            Console.WriteLine("Carta no topo de outros jogadores:");
            MostrarCartasTopo(jogador);

            Console.WriteLine();


            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }


        private void JogarPartida() 
        {
            using (var writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine($"Partida iniciada com {jogadores.Count} jogadores e {monteCompra.ContarCartas()} cartas no monte de compra.");
                writer.Write("Jogadores: ");
                foreach (var jogador in jogadores)
                {
                    writer.Write(jogador.Nome);
                    if (jogador != jogadores.Last()) 
                    {
                        writer.Write(", ");
                    }
                }
                writer.WriteLine();

                int jogadorAtual = 0;
                while (monteCompra.ContarCartas() > 0)
                {
                    Jogador jogador = jogadores[jogadorAtual];

                    bool continuarJogando;
                    do
                    {
                        Carta cartaDaVez = monteCompra.Pop();

                        MostrarCartasTopo(jogador);

                        if (jogador.Monte.Count == 0)
                        {
                            jogador.Monte.Push(cartaDaVez);
                            Console.WriteLine($"{jogador.Nome} não tinha cartas. Pegou a carta {cartaDaVez.DescreverCarta()} e passou a vez.");
                            writer.WriteLine($"{jogador.Nome} não tinha cartas. Pegou a carta {cartaDaVez.DescreverCarta()} e passou a vez.");

                            continuarJogando = false; 
                        }
                        else
                        {
                            ExibirEstadoAtual(jogador, cartaDaVez);
                            writer.WriteLine($"{jogador.Nome} retirou a carta {cartaDaVez.DescreverCarta()}.");

                            continuarJogando = ProcessarJogada(jogador, cartaDaVez, writer);

                            if (!continuarJogando)
                            {
                                areaDescarte.Add(cartaDaVez);
                                writer.WriteLine($"{jogador.Nome} descartou a carta {cartaDaVez.DescreverCarta()}.");
                            }
                        }
                    } while (continuarJogando && monteCompra.ContarCartas() > 0);                    

                    jogadorAtual = (jogadorAtual + 1) % jogadores.Count;
                }

                DeterminarVencedores(writer);
            }
        }





        private void MostrarCartasTopo(Jogador jogadorAtual)
        {

            Console.Write($"Carta no topo do monte de {jogadorAtual.Nome}: ");
            if (jogadorAtual.Monte.Count != 0)
            {
                Console.WriteLine(jogadorAtual.Monte.Peek().DescreverCarta());
            }
            else
            {
                Console.WriteLine("Nenhuma carta");
            }

            foreach (var jogador in jogadores)
            {
                if (jogador != jogadorAtual)
                {
                    Console.Write($"Carta no topo do monte de {jogador.Nome}: ");
                    if (jogador.Monte.Count != 0)
                    {
                        Console.WriteLine(jogador.Monte.Peek().DescreverCarta());
                    }
                    else
                    {
                        Console.WriteLine("Nenhuma carta");
                    }
                }
            }

            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }



        private void DeterminarVencedores(StreamWriter writer)
        {
            int maiorMonte = 0;
            List<Jogador> vencedores = new List<Jogador>();

            foreach (var jogador in jogadores)
            {
                jogador.AtualizarQuantidadeDeCartas();

                if (jogador.QuantidadeCartas > maiorMonte)
                {
                    maiorMonte = jogador.QuantidadeCartas;
                    vencedores.Clear(); 
                    vencedores.Add(jogador);
                }
                else if (jogador.QuantidadeCartas == maiorMonte)
                {
                    vencedores.Add(jogador);
                }
            }

            writer.WriteLine("Resultado da partida:");
            foreach (var vencedor in vencedores)
            {
                writer.WriteLine($"{vencedor.Nome} venceu com {vencedor.QuantidadeCartas} cartas.");
            }

            for (int i = 0; i < jogadores.Count - 1; i++)
            {
                for (int j = i + 1; j < jogadores.Count; j++)
                {
                    if (jogadores[i].QuantidadeCartas < jogadores[j].QuantidadeCartas)
                    {
                        var temp = jogadores[i];
                        jogadores[i] = jogadores[j];
                        jogadores[j] = temp;
                    }
                }
            }

            Console.WriteLine("Ranking Final:");
            for (int i = 0; i < jogadores.Count; i++)
            {

                jogadores[i].Posicao = i + 1;
                Console.WriteLine(jogadores[i].DescreverJogador());
                jogadores[i].AdicionarRanking();

            }
        }

        private bool ProcessarJogada(Jogador jogador, Carta cartaDaVez, StreamWriter writer)
        {
            Jogador monteParaRoubar = null;
            int maiorMonte = -1;

            foreach (var j in jogadores)
            {
                if (j != jogador && j.Monte.Count != 0)
                {
                    if (j.Monte.Peek().Numero == cartaDaVez.Numero)
                    {
                        if (j.Monte.Count > maiorMonte)
                        {
                            monteParaRoubar = j;
                            maiorMonte = j.Monte.Count;
                        }
                    }
                }
            }

            if (monteParaRoubar != null) 
            {
                while (monteParaRoubar.Monte.Count != 0)
                {
                    jogador.Monte.Push(monteParaRoubar.Monte.Pop());
                }
                jogador.Monte.Push(cartaDaVez);
                Console.WriteLine($"{jogador.Nome} roubou o monte de {monteParaRoubar.Nome}!");
                jogador.AtualizarQuantidadeDeCartas();
                writer.WriteLine($"{jogador.Nome} roubou o monte de {monteParaRoubar.Nome}.");
                return true;
            }

            Carta cartaDescarte = null;
            foreach (var carta in areaDescarte)
            {
                if (carta.Numero == cartaDaVez.Numero)
                {
                    cartaDescarte = carta;
                    break;
                }
            }
            if (cartaDescarte != null) 
            {
                jogador.Monte.Push(cartaDescarte);
                areaDescarte.Remove(cartaDescarte);
                jogador.Monte.Push(cartaDaVez);
                jogador.AtualizarQuantidadeDeCartas();
                Console.WriteLine($"{jogador.Nome} pegou a carta {cartaDescarte.DescreverCarta()} da área de descarte.");
                return true;
            }

            if (jogador.Monte.Count == 0 && jogador.Monte.Peek().Numero == cartaDaVez.Numero)
            {
                jogador.Monte.Push(cartaDaVez);
                jogador.AtualizarQuantidadeDeCartas();
                Console.WriteLine($"{jogador.Nome} adicionou a carta {cartaDaVez.DescreverCarta()} ao próprio monte.");
                writer.WriteLine($"{jogador.Nome} adicionou a carta {cartaDaVez.DescreverCarta()} ao próprio monte.");
                return true;
            }

            return false;
        }


    }

        class Program
        {
            static void Main()
            {

            List<Jogador> jogadores = new List<Jogador>();
            bool continuarJogo = true;

            while (continuarJogo)
            {
                if (jogadores.Count == 0)
                {
                    Console.WriteLine("Quantos jogadores participarão?");
                    int quantidadeJogadores = int.Parse(Console.ReadLine());

                    while (quantidadeJogadores <= 0)
                    {
                        Console.WriteLine("Por favor, insira um número válido de jogadores (maior que 0).");
                        quantidadeJogadores = int.Parse(Console.ReadLine());
                    }

                    for (int i = 1; i <= quantidadeJogadores; i++)
                    {
                        Console.Write($"Nome do jogador {i}: ");
                        string nome = Console.ReadLine();
                        jogadores.Add(new Jogador(nome));
                    }
                }

                Console.WriteLine("Quantos baralhos serão usados?");
                int quantidadeBaralhos = int.Parse(Console.ReadLine());
                while (quantidadeBaralhos <= 0)
                {
                    Console.WriteLine("Por favor, insira um número válido de baralhos (maior que 0).");
                    quantidadeBaralhos = int.Parse(Console.ReadLine());
                }

                Jogo jogo = new Jogo(jogadores, quantidadeBaralhos);
                jogo.Iniciar();
                Console.Clear();

                Console.WriteLine("Deseja iniciar um novo jogo? (s/n)");
                string resposta = Console.ReadLine().ToLower();

                while (resposta != "s" && resposta != "n")
                {
                    Console.WriteLine("Resposta inválida. Por favor, digite 's' para sim ou 'n' para não.");
                    resposta = Console.ReadLine().ToLower();
                }

                if (resposta != "s")
                    continuarJogo = false;

            }

            Console.Clear();
            Console.WriteLine("Escolha um jogador para mostrar o ranking das últimas 5 partidas:");
            for (int i = 0; i < jogadores.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jogadores[i].Nome}");
            }

            int escolha = int.Parse(Console.ReadLine());
            Console.WriteLine($"Mostrando o ranking das últimas 5 partidas do jogador: {jogadores[escolha - 1].Nome}");
            jogadores[escolha - 1].MostrarRanking();
            Console.ReadLine(); 
        }
    }
    
 public   class Pilha
    {
        private Carta[] array;
        public int topo;

        public Pilha(int tamanho)
        {
            topo = 0;
            array = new Carta[tamanho];
        }


        public void Push(Carta carta)
        {
            if (topo >= array.Length)
                throw new Exception("Erro!");

            array[topo] = carta;
            topo++;
        }

        public int ContarCartas()
        {
            return topo;
        }

        public Carta Pop()
        {
            if (topo == 0)
                throw new Exception("Erro!");

            topo = topo - 1;
            return array[topo];
        }

    }


   public class Celula
    {
        private int elemento;
        private Celula prox;
        public Celula(int elemento)
        {
            this.elemento = elemento;
            this.prox = null;
        }
        public Celula()
        {
            this.elemento = 0;
            this.prox = null;
        }
        public Celula Prox
        {
            get { return prox; }
            set { prox = value; }
        }
        public int Elemento
        {
            get { return elemento; }
            set { elemento = value; }
        }
    }
    public class Fila
    {
        private Celula primeiro, ultimo;
        private int tamanho;
        public Fila()
        {
            primeiro = new Celula();
            ultimo = primeiro;
        }

        public void Inserir(int x)
        {
            ultimo.Prox = new Celula(x);
            ultimo = ultimo.Prox;
        }

        public int Remover()
        {
            if (primeiro == ultimo)
                throw new Exception("Erro!");
            Celula tmp = primeiro;
            primeiro = primeiro.Prox;
            int elemento = primeiro.Elemento;
            tmp.Prox = null;
            tmp = null;
            return elemento;
        }

        public void Mostrar()
        {
            Console.Write("[");
            for (Celula i = primeiro.Prox; i != null; i = i.Prox)
            {
                Console.Write(i.Elemento + " ");
            }
            Console.WriteLine("]");
        }

         public int Tamanho()
    {
        return tamanho;
    }

    }
}






   
