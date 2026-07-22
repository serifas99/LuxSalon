import 'dart:io';

import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/frizer.dart';
import 'package:ecommerce_desktop/models/termin.dart';
import 'package:ecommerce_desktop/providers/frizer_provider.dart';
import 'package:ecommerce_desktop/providers/termin_provider.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart' show rootBundle;
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:provider/provider.dart';

class IzvjestajiScreen extends StatefulWidget {
  const IzvjestajiScreen({super.key});

  @override
  State<IzvjestajiScreen> createState() => _IzvjestajiScreenState();
}

class _IzvjestajiScreenState extends State<IzvjestajiScreen> {
  late TerminProvider _terminProvider;
  late FrizerProvider _frizerProvider;

  DateTime? _odDatuma;
  DateTime? _doDatuma;

  bool _generisanje = false;

  @override
  void initState() {
    super.initState();
    _terminProvider = context.read<TerminProvider>();
    _frizerProvider = context.read<FrizerProvider>();
  }

  String _formatDate(DateTime? d) {
    if (d == null) return '';
    return "${d.day.toString().padLeft(2, '0')}.${d.month.toString().padLeft(2, '0')}.${d.year}.";
  }

  String _formatDateTime(DateTime? d) {
    if (d == null) return '';
    return "${_formatDate(d)} ${d.hour.toString().padLeft(2, '0')}:${d.minute.toString().padLeft(2, '0')}";
  }

  pw.ThemeData? _pdfTema;

  /// Učitava font koji podržava naša slova (č, ć, š, ž, đ) - podrazumijevani
  /// PDF font (Helvetica) ih ne podržava pa se inače prikazuju kao kvadratići.
  Future<pw.ThemeData> _ucitajPdfTemu() async {
    if (_pdfTema != null) return _pdfTema!;

    final regularData = await rootBundle.load("assets/fonts/DejaVuSans.ttf");
    final boldData = await rootBundle.load("assets/fonts/DejaVuSans-Bold.ttf");

    final regular = pw.Font.ttf(regularData);
    final bold = pw.Font.ttf(boldData);

    _pdfTema = pw.ThemeData.withFont(base: regular, bold: bold);
    return _pdfTema!;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return MasterScreen(
      title: "Izvještaji",
      child: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text("Period (opciono, za izvještaj o terminima)",
                style: theme.textTheme.titleSmall),
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(child: _dateField("Od datuma", _odDatuma, (d) {
                  setState(() => _odDatuma = d);
                })),
                const SizedBox(width: 16),
                Expanded(child: _dateField("Do datuma", _doDatuma, (d) {
                  setState(() => _doDatuma = d);
                })),
              ],
            ),
            const SizedBox(height: 32),
            Wrap(
              spacing: 16,
              runSpacing: 16,
              children: [
                _reportCard(
                  icon: Icons.event_note,
                  title: "Izvještaj o terminima",
                  description:
                      "Pregled svih termina u odabranom periodu sa klijentom, frizerom, uslugom, statusom i cijenom.",
                  onTap: _generisanje ? null : _generisiIzvjestajTermina,
                ),
                _reportCard(
                  icon: Icons.bar_chart,
                  title: "Izvještaj o prihodu po frizeru",
                  description:
                      "Broj odrađenih/potvrđenih termina i ukupan prihod po svakom frizeru.",
                  onTap: _generisanje ? null : _generisiIzvjestajPrihoda,
                ),
              ],
            ),
            if (_generisanje) ...[
              const SizedBox(height: 24),
              const Center(child: CircularProgressIndicator()),
            ],
          ],
        ),
      ),
    );
  }

  Widget _dateField(String label, DateTime? value, ValueChanged<DateTime?> onChanged) {
    return InkWell(
      onTap: () async {
        var picked = await showDatePicker(
          context: context,
          initialDate: value ?? DateTime.now(),
          firstDate: DateTime(2020),
          lastDate: DateTime(2100),
        );
        onChanged(picked);
      },
      child: InputDecorator(
        decoration: InputDecoration(label: Text(label)),
        child: Text(value != null ? _formatDate(value) : "Nije odabrano"),
      ),
    );
  }

  Widget _reportCard({
    required IconData icon,
    required String title,
    required String description,
    required VoidCallback? onTap,
  }) {
    final theme = Theme.of(context);
    return SizedBox(
      width: 320,
      child: Card(
        elevation: 4,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: BorderSide(color: theme.colorScheme.primaryContainer),
        ),
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(icon, size: 32, color: theme.colorScheme.primary),
                const SizedBox(height: 12),
                Text(title, style: theme.textTheme.titleMedium),
                const SizedBox(height: 8),
                Text(description, style: theme.textTheme.bodySmall),
                const SizedBox(height: 12),
                Align(
                  alignment: Alignment.centerRight,
                  child: ElevatedButton.icon(
                    onPressed: onTap,
                    icon: const Icon(Icons.picture_as_pdf),
                    label: const Text("Generiši PDF"),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Future _generisiIzvjestajTermina() async {
    setState(() => _generisanje = true);
    try {
      var termini = await _terminProvider.get(filter: {"pageSize": 1000});
      var items = (termini.items ?? []).where((t) {
        if (_odDatuma != null &&
            t.datumVrijeme != null &&
            t.datumVrijeme!.isBefore(_odDatuma!)) {
          return false;
        }
        if (_doDatuma != null &&
            t.datumVrijeme != null &&
            t.datumVrijeme!.isAfter(_doDatuma!.add(const Duration(days: 1)))) {
          return false;
        }
        return true;
      }).toList();

      items.sort((a, b) =>
          (a.datumVrijeme ?? DateTime(2000)).compareTo(b.datumVrijeme ?? DateTime(2000)));

      final doc = pw.Document(theme: await _ucitajPdfTemu());

      doc.addPage(
        pw.MultiPage(
          pageFormat: PdfPageFormat.a4,
          header: (context) => pw.Column(
            crossAxisAlignment: pw.CrossAxisAlignment.start,
            children: [
              pw.Text("LuxSalon - Izvještaj o terminima",
                  style: pw.TextStyle(fontSize: 18, fontWeight: pw.FontWeight.bold)),
              pw.Text(
                  "Period: ${_odDatuma != null ? _formatDate(_odDatuma) : 'početak'} - ${_doDatuma != null ? _formatDate(_doDatuma) : 'danas'}"),
              pw.Text("Generisano: ${_formatDateTime(DateTime.now())}"),
              pw.SizedBox(height: 12),
            ],
          ),
          build: (context) => [
            pw.Table.fromTextArray(
              headers: ["Klijent", "Frizer", "Usluga", "Datum i vrijeme", "Cijena", "Status"],
              data: items
                  .map((t) => [
                        t.klijentImePrezime ?? '',
                        t.frizerImePrezime ?? '',
                        t.uslugaNaziv ?? '',
                        _formatDateTime(t.datumVrijeme),
                        t.cijena != null ? "${t.cijena} KM" : '',
                        t.status ?? '',
                      ])
                  .toList(),
              headerStyle: pw.TextStyle(fontWeight: pw.FontWeight.bold, fontSize: 9),
              cellStyle: const pw.TextStyle(fontSize: 8),
              cellAlignment: pw.Alignment.centerLeft,
            ),
            pw.SizedBox(height: 16),
            pw.Text("Ukupno termina: ${items.length}",
                style: pw.TextStyle(fontWeight: pw.FontWeight.bold)),
          ],
        ),
      );

      await _sacuvajIOtvoriPdf(doc, "izvjestaj_termini");
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    } finally {
      if (mounted) setState(() => _generisanje = false);
    }
  }

  /// Snima PDF u privremeni folder i otvara ga u podrazumijevanom
  /// pregledniku (npr. Edge/Adobe), umjesto da se otvori Windows Print dijalog.
  Future _sacuvajIOtvoriPdf(pw.Document doc, String nazivPrefiks) async {
    final bytes = await doc.save();
    final fileName =
        "${nazivPrefiks}_${DateTime.now().millisecondsSinceEpoch}.pdf";
    final file =
        File("${Directory.systemTemp.path}${Platform.pathSeparator}$fileName");
    await file.writeAsBytes(bytes);

    if (Platform.isWindows) {
      await Process.run('cmd', ['/c', 'start', '', file.path]);
    } else {
      await Process.run('open', [file.path]);
    }
  }

  Future _generisiIzvjestajPrihoda() async {
    setState(() => _generisanje = true);
    try {
      var termini = await _terminProvider.get(filter: {"pageSize": 1000});
      var frizeri = await _frizerProvider.get(filter: {"pageSize": 1000});

      var relevantni = (termini.items ?? [])
          .where((t) => t.status == "Odradjen" || t.status == "Potvrdjen")
          .toList();

      Map<int, List<Termin>> poFrizeru = {};
      for (var t in relevantni) {
        if (t.frizerId == null) continue;
        poFrizeru.putIfAbsent(t.frizerId!, () => []).add(t);
      }

      final doc = pw.Document(theme: await _ucitajPdfTemu());
      double ukupno = 0;

      List<List<String>> redovi = [];
      for (var f in (frizeri.items ?? [])) {
        var listaTermina = poFrizeru[f.id] ?? [];
        double prihod = listaTermina.fold(0.0, (sum, t) => sum + (t.cijena ?? 0));
        ukupno += prihod;
        redovi.add([
          f.imePrezime ?? '',
          listaTermina.length.toString(),
          "${prihod.toStringAsFixed(2)} KM",
        ]);
      }

      doc.addPage(
        pw.MultiPage(
          pageFormat: PdfPageFormat.a4,
          header: (context) => pw.Column(
            crossAxisAlignment: pw.CrossAxisAlignment.start,
            children: [
              pw.Text("LuxSalon - Izvještaj o prihodu po frizeru",
                  style: pw.TextStyle(fontSize: 18, fontWeight: pw.FontWeight.bold)),
              pw.Text("Uključeni statusi: Potvrđen, Odrađen"),
              pw.Text("Generisano: ${_formatDateTime(DateTime.now())}"),
              pw.SizedBox(height: 12),
            ],
          ),
          build: (context) => [
            pw.Table.fromTextArray(
              headers: ["Frizer", "Broj termina", "Ukupan prihod"],
              data: redovi,
              headerStyle: pw.TextStyle(fontWeight: pw.FontWeight.bold, fontSize: 10),
              cellStyle: const pw.TextStyle(fontSize: 9),
            ),
            pw.SizedBox(height: 16),
            pw.Text("Ukupan prihod salona: ${ukupno.toStringAsFixed(2)} KM",
                style: pw.TextStyle(fontWeight: pw.FontWeight.bold)),
          ],
        ),
      );

      await _sacuvajIOtvoriPdf(doc, "izvjestaj_prihod_po_frizeru");
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    } finally {
      if (mounted) setState(() => _generisanje = false);
    }
  }
}
