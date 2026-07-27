import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/providers/frizer_provider.dart';
import 'package:ecommerce_desktop/providers/termin_provider.dart';
import 'package:ecommerce_desktop/providers/usluga_provider.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  late TerminProvider _terminProvider;
  late UslugaProvider _uslugaProvider;
  late FrizerProvider _frizerProvider;

  bool isLoading = true;

  int ukupnoTermina = 0;
  int zakazani = 0;
  int potvrdjeni = 0;
  int odradjeni = 0;
  int otkazani = 0;
  int nijeSeOdazvao = 0;
  int ukupnoUsluga = 0;
  int ukupnoFrizera = 0;
  double ukupanPrihod = 0;

  // Broj termina po danu u sedmici (1=Ponedjeljak ... 5=Petak), za grafikon
  // "Broj termina po danima" - po uzoru na mockup iz prijave teme.
  static const List<String> _naziviDana = [
    "Ponedjeljak",
    "Utorak",
    "Srijeda",
    "Četvrtak",
    "Petak",
  ];
  List<int> terminiPoDanu = List.filled(5, 0);

  @override
  void initState() {
    super.initState();
    _terminProvider = context.read<TerminProvider>();
    _uslugaProvider = context.read<UslugaProvider>();
    _frizerProvider = context.read<FrizerProvider>();
    loadData();
  }

  Future loadData() async {
    try {
      var termini = await _terminProvider.get(filter: {"pageSize": 100});
      var usluge = await _uslugaProvider.get(filter: {"pageSize": 100});
      var frizeri = await _frizerProvider.get(filter: {"pageSize": 100});

      var items = termini.items ?? [];

      setState(() {
        ukupnoTermina = items.length;
        zakazani = items.where((t) => t.status == "Zakazan").length;
        potvrdjeni = items.where((t) => t.status == "Potvrdjen").length;
        odradjeni = items.where((t) => t.status == "Odradjen").length;
        otkazani = items.where((t) => t.status == "Otkazan").length;
        nijeSeOdazvao = items.where((t) => t.status == "NijeSeOdazvao").length;
        ukupnoUsluga = usluge.items?.length ?? 0;
        ukupnoFrizera = frizeri.items?.length ?? 0;
        ukupanPrihod = items
            .where((t) => t.status == "Odradjen" || t.status == "Potvrdjen")
            .fold(0.0, (sum, t) => sum + (t.cijena ?? 0));

        var brojac = List.filled(5, 0);
        for (var t in items) {
          final dan = t.datumVrijeme?.weekday; // 1=Ponedjeljak ... 7=Nedjelja
          if (dan != null && dan >= 1 && dan <= 5) {
            brojac[dan - 1]++;
          }
        }
        terminiPoDanu = brojac;

        isLoading = false;
      });
    } on Exception catch (e) {
      alertBox(context, 'Greška', e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: "Pregled",
      child: isLoading
          ? const Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(16.0),
              child: SingleChildScrollView(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Wrap(
                      spacing: 16,
                      runSpacing: 16,
                      children: [
                        _card("Ukupno termina", ukupnoTermina.toString(),
                            Icons.event, Colors.indigo),
                        _card("Zakazani", zakazani.toString(), Icons.schedule,
                            Colors.orange),
                        _card("Potvrđeni", potvrdjeni.toString(),
                            Icons.check_circle_outline, Colors.blue),
                        _card("Odrađeni", odradjeni.toString(), Icons.done_all,
                            Colors.green),
                        _card("Otkazani", otkazani.toString(), Icons.cancel,
                            Colors.red),
                        _card("Nije se odazvao", nijeSeOdazvao.toString(),
                            Icons.person_off, Colors.brown),
                        _card("Usluge", ukupnoUsluga.toString(),
                            Icons.content_cut, Colors.purple),
                        _card("Frizeri", ukupnoFrizera.toString(),
                            Icons.people, Colors.teal),
                        _card("Procijenjeni prihod",
                            "${ukupanPrihod.toStringAsFixed(2)} KM",
                            Icons.payments, Colors.green.shade800),
                      ],
                    ),
                    const SizedBox(height: 24),
                    _brojTerminaPoDanimaChart(),
                  ],
                ),
              ),
            ),
    );
  }

  Widget _brojTerminaPoDanimaChart() {
    final theme = Theme.of(context);
    final maxY = (terminiPoDanu.isEmpty
                ? 0
                : terminiPoDanu.reduce((a, b) => a > b ? a : b))
            .toDouble() *
        1.2;

    return Container(
      width: 480,
      padding: const EdgeInsets.fromLTRB(16, 16, 24, 12),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: theme.colorScheme.outlineVariant),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text("Broj termina po danima",
              style: theme.textTheme.titleMedium
                  ?.copyWith(fontWeight: FontWeight.bold)),
          const SizedBox(height: 16),
          SizedBox(
            height: 220,
            child: LineChart(
              LineChartData(
                minY: 0,
                maxY: maxY <= 0 ? 5 : maxY,
                gridData: const FlGridData(show: true, drawVerticalLine: false),
                borderData: FlBorderData(show: false),
                titlesData: FlTitlesData(
                  topTitles: const AxisTitles(
                      sideTitles: SideTitles(showTitles: false)),
                  rightTitles: const AxisTitles(
                      sideTitles: SideTitles(showTitles: false)),
                  leftTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      reservedSize: 32,
                      getTitlesWidget: (value, meta) =>
                          Text(value.toInt().toString(),
                              style: const TextStyle(fontSize: 11)),
                    ),
                  ),
                  bottomTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      reservedSize: 28,
                      getTitlesWidget: (value, meta) {
                        final i = value.toInt();
                        if (i < 0 || i >= _naziviDana.length) {
                          return const SizedBox.shrink();
                        }
                        return Padding(
                          padding: const EdgeInsets.only(top: 6),
                          child: Text(_naziviDana[i].substring(0, 3),
                              style: const TextStyle(fontSize: 11)),
                        );
                      },
                    ),
                  ),
                ),
                lineBarsData: [
                  LineChartBarData(
                    spots: [
                      for (int i = 0; i < terminiPoDanu.length; i++)
                        FlSpot(i.toDouble(), terminiPoDanu[i].toDouble()),
                    ],
                    isCurved: true,
                    color: Colors.brown.shade400,
                    barWidth: 3,
                    dotData: const FlDotData(show: true),
                    belowBarData: BarAreaData(
                      show: true,
                      color: Colors.brown.withOpacity(0.12),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _card(String title, String value, IconData icon, Color color) {
    return Container(
      width: 220,
      padding: const EdgeInsets.all(16.0),
      decoration: BoxDecoration(
        color: color.withOpacity(0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: color),
          const SizedBox(height: 12),
          Text(value,
              style: TextStyle(
                  fontSize: 24, fontWeight: FontWeight.bold, color: color)),
          const SizedBox(height: 4),
          Text(title, style: TextStyle(color: Colors.grey.shade700)),
        ],
      ),
    );
  }
}
