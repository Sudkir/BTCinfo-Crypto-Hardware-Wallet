using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NBitcoin;
using Key = NBitcoin.Key;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System.Collections;

namespace BTCeks
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///

    public partial class MainWindow : Window
    {
        public void puKeyGen()
        {
            Key privateKey = new Key(); //случайный приватный ключ
            PubKey publicKey = privateKey.PubKey; //открытый ключ из приватного

            string StrAddress = publicKey.GetAddress(ScriptPubKeyType.Legacy, Network.TestNet).ToString();
            MessageBox.Show(StrAddress, "Test address");

            //Console.WriteLine(publicKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main)); // 1PUYsjwfNmX64wS368ZR5FMouTtUmvtmTY
            //Console.WriteLine(publicKey.GetAddress(ScriptPubKeyType.Legacy, Network.TestNet)); // n3zWAo2eBnxLr3ueohXnuAa8mTVBhxmPhq

            var publicKeyHash = publicKey.Hash; //PubKey hash
            var paymentScript = publicKeyHash.ScriptPubKey; // cпособ индификации внутри сети биткоин
            MessageBox.Show(paymentScript.ToString(), "Test address");

            //получения адреса из Key hash
            var mainNetAddress = publicKeyHash.GetAddress(Network.Main);
            var testNetAddress = publicKeyHash.GetAddress(Network.TestNet);
            //получения адреса из ScriptPubKey и сети
            var sameMainNetAddress = paymentScript.GetDestinationAddress(Network.Main);

            // формат импорта кошелька WIF
            BitcoinSecret testNetPrivateKey = privateKey.GetBitcoinSecret(Network.TestNet);
            bool WifIsBitcoinSecret = testNetPrivateKey == privateKey.GetWif(Network.TestNet);
        }

        public void his()
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.Main);
            // Create a client

            //Transaction tx = Transaction.Parse("9583ef0d5f15877008125ba3a6827ad4f7789aab415c29ea0646c98eb33b4032", Network.Main);

            // Parse transaction id to NBitcoin.uint256 ХЭШ транзакции для просмотра информации
            var transactionId = uint256.Parse("bd02819e5f81e5f78e18a33ffdc5e9a7d4aa9f6c4cc519a768dc11e6771b33d3");
            //var transactionId = uint256.Parse("f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94");
            // Query the transaction
            GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;
            // get NBitcoin.Transaction type
            NBitcoin.Transaction transaction = transactionResponse.Transaction;

            List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;
            List<ICoin> spentCoins = transactionResponse.SpentCoins;
        }

        public void TransactionIdQuery(string wallet)
        {
            //QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            QBitNinjaClient client = new QBitNinjaClient(Network.Main);
            var coinsReceived = client.GetBalance(BitcoinAddress.Create(wallet, Network.Main), true).Result;
            //var s = client.GetWalletClient();

            //список кому отправили деньги
            ArrayList myAL = new ArrayList();
            ArrayList myALd = new ArrayList();

            //история получения
            var en = coinsReceived.Operations;

            foreach (var entry in coinsReceived.Operations)
            {
                //проверка каждой через консоль

                foreach (var coin in entry.ReceivedCoins)
                {
                    Money amount = (Money)coin.Amount;

                    var paymentScript = coin.TxOut.ScriptPubKey;  // It's the ScriptPubKey

                    var address = paymentScript.GetDestinationAddress(Network.Main);

                    decimal payB = amount.ToDecimal(MoneyUnit.BTC);
                    string payAddress = address.ToString();
                    string payTrans = payAddress + "  -> " + payB.ToString();
                    myAL.Add(payTrans);
                }
                myAL.Add("--");
            }
            QBitNinjaClient client1 = new QBitNinjaClient(Network.Main);
            var coinsSpent = client1.GetBalance(BitcoinAddress.Create(wallet, Network.Main), true).Result;
            //история отправления
            foreach (var ex in coinsSpent.Operations)
            {
                foreach (var coin in ex.SpentCoins)
                {
                    Money amount = (Money)coin.Amount;

                    var paymentScript = coin.TxOut.ScriptPubKey;  // It's the ScriptPubKey

                    var address = paymentScript.GetDestinationAddress(Network.Main);

                    decimal payB = amount.ToDecimal(MoneyUnit.BTC);
                    string payAddress = address.ToString();
                    string payTrans = payAddress + "  -> " + payB.ToString();
                    myALd.Add(payTrans);
                }
                myALd.Add("--");
            }
            SellCoin.ItemsSource = myAL;
            //GetCoin.ItemsSource = myALd;
        }

        public void Balance(string wallet)
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.Main);
            decimal totalBalance = 0;

            // TODO: Calculate the Total Balance.
            var balance = client.GetBalance(BitcoinAddress.Create(wallet, Network.Main), true).Result;
            foreach (var entry in balance.Operations)
            {
                foreach (var coin in entry.ReceivedCoins)
                {
                    Money amount = (Money)coin.Amount;
                    decimal currentAmount = amount.ToDecimal(MoneyUnit.BTC);
                    totalBalance += currentAmount;
                }
            }

            BalanceTxt.Text = totalBalance.ToString();
        }

        //public void HistoreNew()
        //{
        //    // 0. Запросить все операции и классифицировать используемые безопасные адреса (адреса кошельков) по группам
        //    //Dictionary<BitcoinAddress, List<BalanceOperation>> operationsPerAddresses = QueryOperationsPerSafeAddresses(safe);

        //    Dictionary<uint256, List<BalanceOperation>> operationsPerTransactions = GetOperationsPerTransactions(operationsPerAddresses);

        //    // 3. Записываем историю транзакций
        //    // Функция показа информации истории пользователя не обязательна
        //    var txHistoryRecords = new List<Tuple<DateTimeOffset, Money, int, uint256>>();
        //    foreach (var elem in operationsPerTransactions)
        //    {
        //        var amount = Money.Zero;
        //        foreach (var op in elem.Value)
        //            amount += op.Amount;
        //        var firstOp = elem.Value.First();

        //        txHistoryRecords
        //            .Add(new Tuple<DateTimeOffset, Money, int, uint256>(
        //                firstOp.FirstSeen,
        //                amount,
        //                firstOp.Confirmations,
        //                elem.Key));
        //    }

        //    // 4. Сортировать записи по времени или порядку подтверждения (сортировка по времени недопустима, потому что в QBitNinja есть такая ошибка)
        //    var orderedTxHistoryRecords = txHistoryRecords
        //             .OrderByDescending(x => x.Item3) // Сортировка по времени
        //             .ThenBy(x => x.Item1); // первый элемент
        //    foreach (var record in orderedTxHistoryRecords)
        //    {
        //        MessageBox.Show(record.Item2.Satoshi.ToString());
        //        }
        //}

        public MainWindow()
        {
            InitializeComponent();

            //puKeyGen();
            //TransactionIdQuery("12cbQLTFMXRnSzktFkuoG3eHoMeFtpTu3S");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string adre = AddressTxt.Text;
            TransactionIdQuery(adre);
            Balance(adre);
        }
    }
}