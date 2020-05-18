using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace CSP
{
    public partial class Form1 : Form
    {
        List<List<double>> emlak = new List<List<double>>();
        List<Veri> liste = new List<Veri>(); //çektiğim verileri tuttuğum liste
        List<double> l = new List<double>();
        double[,] agirlik = new double[8, 100]; //weight matrisim
        double ogrenmekatsayisi = 0.3;//öğrenme katsayım
        double toplamiterasyon = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            toplamiterasyon = Convert.ToDouble((textBox1.Text), CultureInfo.InvariantCulture.NumberFormat);
            //dosyadan okuma yapılan kısım
            string satir;
            StreamReader dosya = new StreamReader("emlak-veri.txt");
            satir = dosya.ReadLine();
            while ((satir = dosya.ReadLine()) != null)
            {
                //CultureInfo.InvariantCulture.NumberFormat dopsya okurken convertde . varsa eğer 12.5 i 125 diye okuyordu ve min max sıkıntı çıkartıyordu
                Veri veri = new Veri();
                string[] words = satir.Split(',');
                veri.areaLandzone = Convert.ToDouble(words[0], CultureInfo.InvariantCulture.NumberFormat);
                veri.nonRetail = Convert.ToDouble(words[1], CultureInfo.InvariantCulture.NumberFormat);
                veri.averRooms = Convert.ToDouble(words[2], CultureInfo.InvariantCulture.NumberFormat);
                veri.ownerOccupied = Convert.ToDouble(words[3], CultureInfo.InvariantCulture.NumberFormat);
                veri.indexHighways = words[4];
                veri.tax = Convert.ToDouble(words[5], CultureInfo.InvariantCulture.NumberFormat);
                veri.ptratio = Convert.ToDouble(words[6], CultureInfo.InvariantCulture.NumberFormat);
                veri.medianVal = Convert.ToDouble(words[7], CultureInfo.InvariantCulture.NumberFormat);
                liste.Add(veri);
            }
            dosya.Close();
            randomDoldur();
            minMax();
            duzenle();
            findAndUpdateWeights();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
        }
        private void duzenle()
        {

            for (int i = 0; i < 500; i++)
            {
                List<double> yedek = new List<double>();
                yedek.Add(liste[i].areaLandzone);
                yedek.Add(liste[i].nonRetail);
                yedek.Add(liste[i].averRooms);
                yedek.Add(liste[i].ownerOccupied);
                yedek.Add(Convert.ToDouble(liste[i].indexHighways));
                yedek.Add(liste[i].tax);
                yedek.Add(liste[i].ptratio);
                yedek.Add(liste[i].medianVal);
                emlak.Add(yedek);
            }
        }
        //ilk weightleri random doldurma fonksiyonu
        private void randomDoldur()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < agirlik.GetLength(0); i++)
            {
                for (int j = 0; j < agirlik.GetLength(1); j++)
                {
                    agirlik[i, j] = random.NextDouble() * (1 - 0) + 0;
                }
            }
        }
        //min max yapılan kısım
        private void minMax()
        {
            List<double> area_landzonelist = new List<double>();
            List<double> non_retaillist = new List<double>();
            List<double> aver_roomslist = new List<double>();
            List<double> owner_occupiedlist = new List<double>();
            List<double> taxlist = new List<double>();
            List<double> ptratiolist = new List<double>();
            List<double> median_vallist = new List<double>();
            List<string> index_highwayslist = new List<string>();
            List<string> index_unique = new List<string>();
            //yukarıdakileri her biri ayrı parametre olduğu için açtım
            foreach (Veri item in liste)
            {
                //her parametre tek tek ekleniyor listeye
                area_landzonelist.Add(item.areaLandzone);
                non_retaillist.Add(item.nonRetail);
                aver_roomslist.Add(item.averRooms);
                owner_occupiedlist.Add(item.ownerOccupied);
                taxlist.Add(item.tax);
                ptratiolist.Add(item.ptratio);
                median_vallist.Add(item.medianVal);
                index_highwayslist.Add(item.indexHighways);
            }
            foreach (Veri item1 in liste)
            {
                //string değerler hariç hepsi kendi listesi üzerinden update ediliyor
                item1.areaLandzone = (item1.areaLandzone - area_landzonelist.Min()) / (area_landzonelist.Max() - area_landzonelist.Min());
                item1.nonRetail = (item1.nonRetail - non_retaillist.Min()) / (non_retaillist.Max() - non_retaillist.Min());
                item1.averRooms = (item1.averRooms - aver_roomslist.Min()) / (aver_roomslist.Max() - aver_roomslist.Min());
                item1.ownerOccupied = (item1.ownerOccupied - owner_occupiedlist.Min()) / (owner_occupiedlist.Max() - owner_occupiedlist.Min());
                item1.tax = (item1.tax - taxlist.Min()) / (taxlist.Max() - taxlist.Min());
                item1.ptratio = (item1.ptratio - ptratiolist.Min()) / (ptratiolist.Max() - ptratiolist.Min());
                item1.medianVal = (item1.medianVal - median_vallist.Min()) / (median_vallist.Max() - median_vallist.Min());
            }
            //stringlerin uniq olaranlarını bulduğum kısım 8 tane varmış sonra elle kontrol ettim
            for (int i = 0; i < index_highwayslist.Count; i++)
            {
                if (index_unique == null)
                {
                    index_unique.Add(index_highwayslist[i]);
                }
                else if (!index_unique.Contains(index_highwayslist[i]))
                {
                    index_unique.Add(index_highwayslist[i]);
                }
            }
            double[] index_count = new double[index_unique.Count];
            //uniquelerden kaç tane var diye kontrol ediyorum
            for (int j = 0; j < index_unique.Count; j++)
            {
                for (int k = 0; k < index_highwayslist.Count; k++)
                {
                    if (index_unique[j] == index_highwayslist[k])
                    {
                        index_count[j]++;
                    }
                }
            }
            //hangi harften kaçtane varsa o kısma o eleman sayısının bütün sayıya oranını yazıyorum en başta 
            foreach (Veri item2 in liste)
            {
                //8 tane olduğunu bildiğim için switch case içinde sabit değer istediği için caselerde elle if else if yaptığım kısım
                double x = index_count.Max();
                double y = index_count.Min();
                if (item2.indexHighways == index_unique[0])
                {
                    item2.indexHighways = ((index_count[0] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }

                else if (item2.indexHighways == index_unique[1])
                {
                    item2.indexHighways = ((index_count[1] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[2])
                {
                    item2.indexHighways = ((index_count[2] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[3])
                {
                    item2.indexHighways = ((index_count[3] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[4])
                {
                    item2.indexHighways = ((index_count[4] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[5])
                {
                    item2.indexHighways = ((index_count[5] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[6])
                {
                    item2.indexHighways = ((index_count[6] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[7])
                {
                    item2.indexHighways = ((index_count[7] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
                else if (item2.indexHighways == index_unique[8])
                {
                    item2.indexHighways = ((index_count[8] - index_count.Min()) / (index_count.Max() - index_count.Min())).ToString();
                }
            }
        }
        private void findAndUpdateWeights()
        {
            lDoldur();
            while (toplamiterasyon > 0)
            {
                for (int i = 0; i < 500; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        l[j] = lHesapla(i, j);
                    }
                    int index = l.IndexOf(l.Min());
                    kazananGuncelle(index, i);
                    //1 radius etrafını güncelliyorum
                    if ((index + 1) % 10 > 0)
                        komsuGuncelle(index, i, index + 1);
                    if (index % 10 > 0)
                        komsuGuncelle(index, i, index - 1);
                    if (index - 10 >= 0)
                        komsuGuncelle(index, i, index - 10);
                    if (index + 10 < 100)
                        komsuGuncelle(index, i, index + 10);
                }
                if (ogrenmekatsayisi >= 0.1)
                {
                    ogrenmekatsayisi *= 0.994;
                }
                toplamiterasyon--;
            }

        }
        //kazanan weight güncellenen kısım
        private void kazananGuncelle(int index, int i)
        {
            agirlik[0, index] = agirlik[0, index] + (ogrenmekatsayisi * (liste[i].areaLandzone - agirlik[0, index]));
            agirlik[1, index] = agirlik[1, index] + (ogrenmekatsayisi * (liste[i].nonRetail - agirlik[1, index]));
            agirlik[2, index] = agirlik[2, index] + (ogrenmekatsayisi * (liste[i].averRooms - agirlik[2, index]));
            agirlik[3, index] = agirlik[3, index] + (ogrenmekatsayisi * (liste[i].ownerOccupied - agirlik[3, index]));
            agirlik[4, index] = agirlik[4, index] + (ogrenmekatsayisi * (Convert.ToDouble(liste[i].indexHighways) - agirlik[4, index]));
            agirlik[5, index] = agirlik[5, index] + (ogrenmekatsayisi * (liste[i].tax - agirlik[5, index]));
            agirlik[6, index] = agirlik[6, index] + (ogrenmekatsayisi * (liste[i].ptratio - agirlik[6, index]));
            agirlik[7, index] = agirlik[7, index] + (ogrenmekatsayisi * (liste[i].medianVal - agirlik[7, index]));
        }
        //bmu bulmak için hesaplama yapılan kısım
        private double lHesapla(int i, int j)
        {
            double x = 0;
            x = Math.Pow((liste[i].areaLandzone - agirlik[0, j]), 2) + Math.Pow((liste[i].nonRetail - agirlik[1, j]), 2) +
                         Math.Pow((liste[i].averRooms - agirlik[2, j]), 2) + Math.Pow((liste[i].ownerOccupied - agirlik[3, j]), 2) +
                         Math.Pow((Convert.ToDouble(liste[i].indexHighways) - agirlik[4, j]), 2) + Math.Pow((liste[i].tax - agirlik[5, j]), 2) +
                         Math.Pow((liste[i].ptratio - agirlik[6, j]), 2) + Math.Pow((liste[i].medianVal - agirlik[7, j]), 2);
            return x;
        }
        //başta null olduğu için ve min aradığımız için hepsini max value ile değiştirdim listeyi sürekliyo değiştiriyorum
        private void lDoldur()
        {
            for (int i = 0; i < 100; i++)
            {
                l.Add(int.MaxValue);
            }
        }
        //kazanan weight etrafını güncelleme kısmı
        private void komsuGuncelle(int index, int input, int komsu)
        {
            double sigma = 2;
            for (int j = 0; j < 8; j++)
            {
                sigma += Math.Exp(-(Math.Pow(agirlik[j, komsu] - agirlik[j, index], 2) / (2 * sigma)));
                agirlik[j, komsu] += ogrenmekatsayisi * sigma * (emlak[input][j] - agirlik[j, komsu]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int secim1 = 0, secim2 = 0;
            chart1.Series.Clear();
            secim1 = comboBox1.SelectedIndex;
            secim2 = comboBox2.SelectedIndex;
            chart1.Series.Add("Emlak_Verileri");
            chart1.Series.Add("Ağirlik");
            chart1.Series.Add("Komsuluk");

            //point olarak çizdirmek için burası var
            chart1.Series["Emlak_Verileri"].ChartType = 0;
            chart1.Series["Ağirlik"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series["Komsuluk"].ChartType = 0;

            for (int i = 0; i < 500; i++)
            {
                chart1.Series["Emlak_Verileri"].Points.AddXY(emlak[i][secim1], emlak[i][secim2]);
            }

            for (int i = 0; i < 100; i++)
            {
                chart1.Series["Ağirlik"].Points.AddXY(agirlik[secim1, i], agirlik[secim2, i]);
            }
            for (int i = 0; i < 100; i++)
            {
                chart1.Series["Komsuluk"].Points.AddXY(agirlik[secim1, i], agirlik[secim2, i]);
            }
        }
    }
}
