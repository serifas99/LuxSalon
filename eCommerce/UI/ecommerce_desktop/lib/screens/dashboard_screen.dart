import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/providers/frizer_provider.dart';
import 'package:ecommerce_desktop/providers/termin_provider.dart';
import 'package:ecommerce_desktop/providers/usluga_provider.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
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
      var termini = await _terminProvider.get(filter: {"pageSize": 1000});
      var usluge = await _uslugaProvider.get(filter: {"pageSize": 1000});
      var frizeri = await _frizerProvider.get(filter: {"pageSize": 1000});

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
                child: Wrap(
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
              ),
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
