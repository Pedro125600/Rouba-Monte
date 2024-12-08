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

        public string DescreverCarta() // Para na hora de chamar os numeros aparecerem como AS,REi, etc
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

      
        public Fila rankingUltimas5Partidas { get; set; } // Usando a fila personalizada

        public void AdicionarRanking()
        {
            // Insere o resultado e mantém apenas os últimos 5
            if (rankingUltimas5Partidas.Tamanho() == 5)
            {
                rankingUltimas5Partidas.Remover(); // Remove o mais antigo
            }
            rankingUltimas5Partidas.Inserir(Posicao);
        }

        public void MostrarRanking()
        {
            Console.WriteLine("Ranking das últimas 5 partidas:");
            rankingUltimas5Partidas.Mostrar();
        }

        public Stack<Carta> Monte { get; private set; } = new Stack<Carta>();


        public void AtualizarQuantidadeDeCartas()// UM metodo qe fiz para contar a quantidade de cartas a toda rodada dos jogadores e ir atualizando o rakimg mas ainda não fiz o ranking 
        {
            this.QuantidadeCartas = Monte.Count;
        }

        public Jogador(string nome)
        {
            Nome = nome;
            rankingUltimas5Partidas = new Fila();
        }



        public string DescreverJogador()//So e usado no final para aparecer os rankings 
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
            int tamanhoMonte = quantidadeBaralhos * 52; //Numero de cartas do baralho veses o numero de baralhos 
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

            // Adiciona as cartas da pilha na lista temporária
            while (monteCompra.topo > 0)
            {
                temp.Add(monteCompra.Pop());
            }

            // Embaralha as cartas
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
                jogador.Monte.Clear(); // Limpa o monte do jogador
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

        private void ExibirEstadoAtual(Jogador jogador, Carta cartaDaVez)//Metodo para exibir o estado do jogador que esta jogando 
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


        private void JogarPartida() // Iniciar partida com o log de partidas para salvar em um arquivo 
        {
            using (var writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine($"Partida iniciada com {jogadores.Count} jogadores e {monteCompra.ContarCartas()} cartas no monte de compra.");
                writer.Write("Jogadores: ");
                foreach (var jogador in jogadores)
                {
                    writer.Write(jogador.Nome);
                    if (jogador != jogadores.Last()) // Adiciona uma vírgula apenas se não for o último jogador
                    {
                        writer.Write(", ");
                    }
                }
                writer.WriteLine(); // Finaliza a linha

                int jogadorAtual = 0;
                while (monteCompra.ContarCartas() > 0) // Continua até o monte de compras acabar
                {
                    Jogador jogador = jogadores[jogadorAtual];

                    bool continuarJogando;
                    do
                    {
                        Carta cartaDaVez = monteCompra.Pop();

                        // Exibe cartas no topo do jogador 
                        MostrarCartasTopo(jogador);

                        // Se o jogador não tiver nenhuma carta, pega a carta da vez e passa para o próximo
                        if (jogador.Monte.Count == 0)
                        {
                            jogador.Monte.Push(cartaDaVez);
                            Console.WriteLine($"{jogador.Nome} não tinha cartas. Pegou a carta {cartaDaVez.DescreverCarta()} e passou a vez.");
                            writer.WriteLine($"{jogador.Nome} não tinha cartas. Pegou a carta {cartaDaVez.DescreverCarta()} e passou a vez.");

                            continuarJogando = false; // Não continua jogando
                        }
                        else
                        {
                            // Exibe estado atual
                            ExibirEstadoAtual(jogador, cartaDaVez);
                            writer.WriteLine($"{jogador.Nome} retirou a carta {cartaDaVez.DescreverCarta()}.");

                            // Processa a jogada
                            continuarJogando = ProcessarJogada(jogador, cartaDaVez, writer);

                            // Se a jogada não for válida, descarta a carta
                            if (!continuarJogando)
                            {
                                areaDescarte.Add(cartaDaVez);
                                writer.WriteLine($"{jogador.Nome} descartou a carta {cartaDaVez.DescreverCarta()}.");
                            }
                        }
                    } while (continuarJogando && monteCompra.ContarCartas() > 0); // Continua enquanto a jogada permitir e houver cartas no monte

                    // Passa para o próximo jogador
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

            // Determina o maior número de cartas e os vencedores
            foreach (var jogador in jogadores)
            {
                jogador.AtualizarQuantidadeDeCartas();

                if (jogador.QuantidadeCartas > maiorMonte)
                {
                    maiorMonte = jogador.QuantidadeCartas;
                    vencedores.Clear(); // Limpa a lista, pois há um novo maior monte
                    vencedores.Add(jogador);
                }
                else if (jogador.QuantidadeCartas == maiorMonte)
                {
                    vencedores.Add(jogador);
                }
            }

            // Escreve os vencedores no arquivo
            writer.WriteLine("Resultado da partida:");
            foreach (var vencedor in vencedores)
            {
                writer.WriteLine($"{vencedor.Nome} venceu com {vencedor.QuantidadeCartas} cartas.");
            }

            // Organiza os jogadores em ordem decrescente e exibe o ranking final
            for (int i = 0; i < jogadores.Count - 1; i++)
            {
                for (int j = i + 1; j < jogadores.Count; j++)
                {
                    if (jogadores[i].QuantidadeCartas < jogadores[j].QuantidadeCartas)
                    {
                        // Troca de posição
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
            // 1. Verifica se pode roubar o monte de outro jogador
            Jogador monteParaRoubar = null;
            int maiorMonte = -1;

            foreach (var j in jogadores)
            {
                // Verifica se não é o jogador atual e se o monte do jogador tem cartas
                if (j != jogador && j.Monte.Count != 0)
                {
                    // Verifica se a carta no topo do monte tem o mesmo número da carta da vez
                    if (j.Monte.Peek().Numero == cartaDaVez.Numero)
                    {
                        // Se tiver salva e verifica os outros so ira passar aquele que tiver combinando e com o maior monte 
                        if (j.Monte.Count > maiorMonte)
                        {
                            monteParaRoubar = j;
                            maiorMonte = j.Monte.Count;
                        }
                    }
                }
            }

            if (monteParaRoubar != null)//Entra se tiver algum monte que o jogador pode roubar 
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

            // 2. Verifica se a carta está na área de descarte
            Carta cartaDescarte = null;
            foreach (var carta in areaDescarte)
            {
                if (carta.Numero == cartaDaVez.Numero)
                {
                    cartaDescarte = carta;
                    break;//Carta estando na area de descartas sai 
                }
            }
            if (cartaDescarte != null) // entra nesse if se o jogador a carta da ves estiver na area de descarte e pega ela retornando true 
            {
                jogador.Monte.Push(cartaDescarte);
                areaDescarte.Remove(cartaDescarte);
                jogador.Monte.Push(cartaDaVez);
                jogador.AtualizarQuantidadeDeCartas();
                Console.WriteLine($"{jogador.Nome} pegou a carta {cartaDescarte.DescreverCarta()} da área de descarte.");
                return true;
            }

            //  Verifica se a carta é igual ao topo do próprio monte
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
                    // Solicita a quantidade de jogadores apenas na primeira vez
                    Console.WriteLine("Quantos jogadores participarão?");
                    int quantidadeJogadores = int.Parse(Console.ReadLine());

                    while (quantidadeJogadores <= 0)
                    {
                        Console.WriteLine("Por favor, insira um número válido de jogadores (maior que 0).");
                        quantidadeJogadores = int.Parse(Console.ReadLine());
                    }

                    // Cria os jogadores
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

                // Inicia um novo jogo com os jogadores existentes e a nova quantidade de baralhos
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

