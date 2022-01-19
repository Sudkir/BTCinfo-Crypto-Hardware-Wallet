using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows;
using Key = NBitcoin.Key;

namespace BTCeks
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///

    public partial class MainWindow : Window
    {
        private const string PRIVATE_KEY_PATH = "secret.key";
        private static readonly Network _network = Network.TestNet;

        public void OldpuKeyGen()
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

        public void OldInfoTrans()
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

        public void OldBalance(string wallet)
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

        public void MssGenerateMnemo(out string ssMnemo)
        {
            Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);

            ssMnemo = mnemonic.ToString();
        }

        public void MssGenerateAddress(
        int ssKeynumber,
        out BitcoinAddress ssAddress,
        out Key ssPrivateKey)
        {
            Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);

            string ssMnemo = mnemonic.ToString();

            Mnemonic restoreNnemo = new Mnemonic(ssMnemo);

            ExtKey masterKey = restoreNnemo.DeriveExtKey();

            KeyPath keypth = new KeyPath("m/44'/0'/0'/0/" + ssKeynumber);
            ExtKey key = masterKey.Derive(keypth);

            ssAddress = key.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, _network);
            ssPrivateKey = null;
            //key.PrivateKey.GetBitcoinSecret(net);
        }

        public void ConnectToWallet(out BitcoinAddress Uaddress, out Key Privatekey)
        {
            // recover private key if it was saved and encrypted with password
            Key _privateKey = null;
            if (File.Exists(PRIVATE_KEY_PATH) && Pass.Text != null)
            {
                var sec = new BitcoinEncryptedSecretNoEC(File.ReadAllBytes(PRIVATE_KEY_PATH), _network);
                try
                {
                    _privateKey = sec.GetKey(Pass.Text);

                    // Display wallet address for receiving money
                    var address = _privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, _network);
                    MessageBox.Show("Your BTC address:" + address.ToString());

                    Privatekey = _privateKey;
                    Uaddress = address;
                    Add.Text = address.ToString();
                    // Save private key to local file that's encrypted with password

                    if (Pass != null)
                    {
                        var encKey = _privateKey.GetEncryptedBitcoinSecret(Pass.Text, _network);
                        File.WriteAllBytes(PRIVATE_KEY_PATH, encKey.ToBytes());
                    }
                }
                catch (SecurityException)
                {
                    MessageBox.Show("229");
                    Privatekey = null;
                    Uaddress = null;
                }
            }
            else
            {
                _privateKey = new Key();
                // Display wallet address for receiving money
                var address = _privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, _network);
                MessageBox.Show("Your BTC address:" + address.ToString());

                Privatekey = _privateKey;
                Uaddress = address;
            }
        }

        public static void Pay(Key privateKey, string IdTrans, string toAddress)
        {
            // Replace this with Network.Main to do this on Bitcoin MainNet
            var network = Network.TestNet;

            var bitcoinPrivateKey = privateKey.GetWif(network);

            PubKey publicKey = privateKey.PubKey; //открытый ключ из приватного
            var publicKeyHash = publicKey.Hash; //PubKey hash
            var paymentScript = publicKeyHash.ScriptPubKey; // cпособ индификации внутри сети биткоин

            //получения адреса из ScriptPubKey и сети
            var sameMainNetAddress = paymentScript.GetDestinationAddress(network);

            var address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy);

            var client = new QBitNinjaClient(network);
            var transactionId = uint256.Parse(IdTrans);
            var transactionResponse = client.GetTransaction(transactionId).Result;

            //Console.WriteLine(transactionResponse.TransactionId); // 0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a
            //Console.WriteLine(transactionResponse.Block.Confirmations); // 91
            var receivedCoins = transactionResponse.ReceivedCoins;
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy).ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            string s = "We want to spend outpoint:" + outPointToSpend.N + 1;
            MessageBox.Show(s);

            var transaction = Transaction.Create(network);
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });
            var hallOfTheMakersAddress = new BitcoinPubKeyAddress(address.ToString(), network);
            //transaction.Outputs.Add(Money.Coins(0.0004m), hallOfTheMakersAddress.ScriptPubKey);
            // Send the change back
            transaction.Outputs.Add(new Money(0.00013m, MoneyUnit.BTC), hallOfTheMakersAddress.ScriptPubKey);

            // How much you want to spend
            var hallOfTheMakersAmount = new Money(0.0001m, MoneyUnit.BTC);

            // How much miner fee you want to pay
            /* Depending on the market price and
             * the currently advised mining fee,
             * you may consider to increase or decrease it.
             */
            var minerFee = new Money(0.0001m, MoneyUnit.BTC);

            // How much you want to get back as change
            var txInAmount = (Money)receivedCoins[(int)outPointToSpend.N].Amount;
            var changeAmount = txInAmount - hallOfTheMakersAmount - minerFee;

            transaction.Outputs.Add(hallOfTheMakersAmount, hallOfTheMakersAddress.ScriptPubKey);
            // Send the change back
            var privateScript = privateKey.GetScriptPubKey(ScriptPubKeyType.Legacy);
            transaction.Outputs.Add(changeAmount, privateScript);

            //transaction.Outputs.Add(changeAmount, hallOfTheMakersAddress.ScriptPubKey);

            var message = "Long live NBitcoin and its makers!";
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(Money.Zero, TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes));
            // Get it from the public address
            var address2 = BitcoinAddress.Create(toAddress, network);
            transaction.Inputs[0].ScriptSig = address2.ScriptPubKey;
            transaction.Sign(bitcoinPrivateKey, receivedCoins.ToArray());
            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (!broadcastResponse.Success)
            {
                string ErrorCodeResponse = "ErrorCode: " + broadcastResponse.Error.ErrorCode + "Error message: " + broadcastResponse.Error.Reason;
                MessageBox.Show(ErrorCodeResponse);
            }
            else
            {
                string SuccessResponse = "Success! You can check out the hash of the transaciton in any block explorer:" + transaction.GetHash();
                MessageBox.Show(SuccessResponse);
            }

            //https://programmingblockchain.gitbook.io/programmingblockchain/bitcoin_transfer/spend_your_coin
        }

        public void MssGetBalance(
        string ssAddress,
        bool ssIsUnspentOnly,
        out decimal ssBalance,
        out decimal ssConfirmedBalance)
        {
            QBitNinjaClient client = new QBitNinjaClient(_network);

            var balance = client.GetBalance(BitcoinAddress.Create(ssAddress, _network), ssIsUnspentOnly).Result;

            ssBalance = 0.0M;
            ssConfirmedBalance = 0.0M;

            if (balance.Operations.Count > 0)
            {
                var unspentCoins = new List<Coin>();
                var unspentCoinsConfirmed = new List<Coin>();
                foreach (var operation in balance.Operations)
                {
                    unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                    if (operation.Confirmations > 0)
                        unspentCoinsConfirmed.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                }

                ssBalance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));

                ssConfirmedBalance = unspentCoinsConfirmed.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
            }
        }

        public void TransactionIdQuery(string wallet)
        {
            QBitNinjaClient client = new QBitNinjaClient(_network);
            //список кому отправили деньги
            ArrayList ListTrans = new ArrayList();
            var coinsReceived = client.GetBalance(BitcoinAddress.Create(wallet, _network), true).Result;

            foreach (BalanceOperation entry in coinsReceived.Operations)
            {
                //проверка каждой через консоль

                foreach (ICoin coin in entry.ReceivedCoins)
                {
                    Money amount = (Money)coin.Amount;

                    var address = coin.TxOut.ScriptPubKey.GetDestinationAddress(_network);  // It's the ScriptPubKey

                    ListTrans.Add(
                        address.ToString() + "  <- " +
                        amount.ToDecimal(MoneyUnit.BTC).ToString() + "BTC  " +
                        entry.FirstSeen.LocalDateTime);
                }
                ListTrans.Add("--");
            }
            SellCoin.ItemsSource = ListTrans;
        }

        public MainWindow()
        {
            InitializeComponent();

            //https://www.youtube.com/watch?v=X4ZwRWIF49w
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string BitcoinAddress = AddressTxt.Text;
            TransactionIdQuery(BitcoinAddress);

            decimal BalanceWallet, ConfirBalanceWallet;

            MssGetBalance(BitcoinAddress, true, out BalanceWallet, out ConfirBalanceWallet);
             BalanceTxt.Text = BalanceWallet.ToString();
            ConfirmedBalanceTxt.Text = ConfirBalanceWallet.ToString();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            BitcoinAddress bitcoinAddress;
            Key p;
            ConnectToWallet(out bitcoinAddress, out p);
        }

        private void PayBtn(object sender, RoutedEventArgs e)
        {
            BitcoinAddress bitcoinAddress;
            Key p;
            ConnectToWallet(out bitcoinAddress, out p);
            Pay(p, "845944f6c15f90d46a6263ce2acd333f12ec31011ad4d73f27436df7cc9c3cf6", "tb1ql7w62elx9ucw4pj5lgw4l028hmuw80sndtntxt");
        }
    }
}