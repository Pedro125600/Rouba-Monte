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


    }

    public class Jogador
    {
        public string Nome { get; set; }
        public int Posicao { get; set; }
        public int QuantidadeCartas;
        public Stack<Carta> Monte { get; private set; } = new Stack<Carta>();
        public Queue<int> Ranking { get; private set; } = new Queue<int>();

        public void AtualizarQuantidadeDeCartas()
        {
            this.QuantidadeCartas = Monte.Count;
        }

        public Jogador(string nome)
        {
            Nome = nome;
        }

        public void AtualizarRanking(int posicao)
        {
            if (Ranking.Count == 5)
                Ranking.Dequeue();
            Ranking.Enqueue(posicao);
        }


    }

    public class Jogo
    {
        private List<Jogador> jogadores = new List<Jogador>();
        private Stack<Carta> monteCompra = new Stack<Carta>();
        private List<Carta> areaDescarte = new List<Carta>();
        private int quantidadeBaralhos;
        private string logFilePath = "log_partida.txt";

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

            foreach(Carta carta in monteCompra) { temp.Add(carta); }
            monteCompra.Clear();

            Random RandomizarCartas = new Random();


            while (temp.Count > 0)
            {
                int index = RandomizarCartas.Next(temp.Count);
                monteCompra.Push(temp[index]);
                temp.RemoveAt(index);
            }

        }


        public void Iniciar()
        {
            Console.WriteLine("Quantos jogadores participarão?");
            int quantidadeJogadores = int.Parse(Console.ReadLine());

            Console.WriteLine("Quantos baralhos serão usados?");
            quantidadeBaralhos = int.Parse(Console.ReadLine());

            for (int i = 1; i <= quantidadeJogadores; i++)
            {
                Console.Write($"Nome do jogador {i}: ");
                string nome = Console.ReadLine();
                jogadores.Add(new Jogador(nome));
            }

        }


        class Program
        {
            static void Main()
            {
                Jogo jogo = new Jogo();

            }
        }
    }
}
