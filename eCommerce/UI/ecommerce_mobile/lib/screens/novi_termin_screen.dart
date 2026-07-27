import 'package:ecommerce_mobile/models/dostupnost_dana.dart';
import 'package:ecommerce_mobile/models/frizer.dart';
import 'package:ecommerce_mobile/models/usluga.dart';
import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/providers/termin_provider.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:table_calendar/table_calendar.dart';

/// Ekran za zakazivanje termina - koristi color-coded kalendar (zeleno = slobodno,
/// crveno = zauzeto, sivo = salon/frizer ne radi taj dan) na osnovu RadnoVrijeme
/// podataka i postojecih (ne-otkazanih) termina tog frizera.
class NoviTerminScreen extends StatefulWidget {
  final Usluga usluga;
  final List<Frizer> frizeri;
  final Frizer? odabraniFrizer;

  const NoviTerminScreen({
    super.key,
    required this.usluga,
    required this.frizeri,
    this.odabraniFrizer,
  });

  @override
  State<NoviTerminScreen> createState() => _NoviTerminScreenState();
}

class _NoviTerminScreenState extends State<NoviTerminScreen> {
  late TerminProvider _terminProvider;

  Frizer? _frizer;
  static DateTime get _danas =>
      DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);
  DateTime _fokusiraniMjesec = _danas;
  DateTime? _odabraniDatum;
  String? _odabranoVrijeme;
  final TextEditingController _napomenaController = TextEditingController();

  Map<DateTime, DostupnostDana> _dostupnostPoDanu = {};
  List<String> _slotoviZaDan = [];

  bool _ucitavaKalendar = false;
  bool _ucitavaSlotove = false;
  bool _saving = false;

  int get _klijentId =>
      int.tryParse(AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '') ??
      0;

  @override
  void initState() {
    super.initState();
    _terminProvider = context.read<TerminProvider>();
    _frizer = widget.odabraniFrizer;
    if (_frizer != null) _ucitajDostupnostMjeseca();
  }

  DateTime _kljuc(DateTime d) => DateTime(d.year, d.month, d.day);

  Future _ucitajDostupnostMjeseca() async {
    if (_frizer == null) return;
    setState(() => _ucitavaKalendar = true);
    try {
      final lista = await _terminProvider.dostupnost(
        frizerId: _frizer!.id!,
        uslugaId: widget.usluga.id!,
        godina: _fokusiraniMjesec.year,
        mjesec: _fokusiraniMjesec.month,
      );
      if (!mounted) return;
      setState(() {
        _dostupnostPoDanu = {for (var d in lista) _kljuc(d.datum): d};
        _ucitavaKalendar = false;
      });
    } on Exception catch (e) {
      if (mounted) setState(() => _ucitavaKalendar = false);
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  Future _ucitajSlotoveZaDan(DateTime datum) async {
    if (_frizer == null) return;
    setState(() {
      _odabraniDatum = datum;
      _odabranoVrijeme = null;
      _ucitavaSlotove = true;
      _slotoviZaDan = [];
    });
    try {
      final slotovi = await _terminProvider.dostupniSlotovi(
        frizerId: _frizer!.id!,
        uslugaId: widget.usluga.id!,
        datum: datum,
      );
      if (!mounted) return;
      setState(() {
        _slotoviZaDan = slotovi;
        _ucitavaSlotove = false;
      });
    } on Exception catch (e) {
      if (mounted) setState(() => _ucitavaSlotove = false);
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  Color _bojaDana(DateTime dan) {
    final info = _dostupnostPoDanu[_kljuc(dan)];
    if (info == null) return Colors.grey.shade200;
    if (!info.radi) return Colors.grey.shade300;
    return info.slobodno ? Colors.green.shade300 : Colors.red.shade300;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Novi termin")),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              widget.usluga.naziv ?? '',
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            Text(
              "${widget.usluga.cijena ?? 0} KM • ${widget.usluga.trajanjeMinuta ?? 0} min",
              style: TextStyle(color: Colors.grey.shade600),
            ),
            const SizedBox(height: 24),
            const Text("Frizer", style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            DropdownButtonFormField<Frizer>(
              initialValue: _frizer,
              decoration: const InputDecoration(border: OutlineInputBorder()),
              hint: const Text("Izaberi frizera"),
              items: widget.frizeri
                  .map((f) => DropdownMenuItem(
                        value: f,
                        child: Text(f.imePrezime ?? ''),
                      ))
                  .toList(),
              onChanged: (value) {
                setState(() {
                  _frizer = value;
                  _odabraniDatum = null;
                  _odabranoVrijeme = null;
                  _slotoviZaDan = [];
                  _dostupnostPoDanu = {};
                });
                _ucitajDostupnostMjeseca();
              },
            ),
            const SizedBox(height: 20),
            if (_frizer == null)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 24),
                child: Text(
                  "Izaberite frizera da biste vidjeli dostupne termine.",
                  style: TextStyle(color: Colors.grey.shade600),
                ),
              )
            else ...[
              const Text("Odaberite datum",
                  style: TextStyle(fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              _buildLegenda(),
              const SizedBox(height: 8),
              Stack(
                children: [
                  TableCalendar(
                    firstDay: _danas,
                    lastDay: _danas.add(const Duration(days: 180)),
                    focusedDay: _fokusiraniMjesec,
                    selectedDayPredicate: (day) =>
                        _odabraniDatum != null && isSameDay(day, _odabraniDatum),
                    onDaySelected: (selectedDay, focusedDay) {
                      final info = _dostupnostPoDanu[_kljuc(selectedDay)];
                      if (info == null || !info.radi || !info.slobodno) return;
                      _ucitajSlotoveZaDan(selectedDay);
                    },
                    onPageChanged: (focusedDay) {
                      setState(() => _fokusiraniMjesec = focusedDay);
                      _ucitajDostupnostMjeseca();
                    },
                    calendarFormat: CalendarFormat.month,
                    availableCalendarFormats: const {
                      CalendarFormat.month: 'Mjesec'
                    },
                    headerStyle: const HeaderStyle(
                        formatButtonVisible: false, titleCentered: true),
                    calendarBuilders: CalendarBuilders(
                      defaultBuilder: (context, day, focusedDay) => _dan(day),
                      todayBuilder: (context, day, focusedDay) =>
                          _dan(day, istaknutOkvir: true),
                      selectedBuilder: (context, day, focusedDay) =>
                          _dan(day, odabran: true),
                    ),
                  ),
                  if (_ucitavaKalendar)
                    Positioned.fill(
                      child: Container(
                        color: Colors.white.withValues(alpha: 0.6),
                        child: const Center(child: CircularProgressIndicator()),
                      ),
                    ),
                ],
              ),
              const SizedBox(height: 20),
              if (_odabraniDatum != null) ...[
                const Text("Vrijeme",
                    style: TextStyle(fontWeight: FontWeight.bold)),
                const SizedBox(height: 8),
                if (_ucitavaSlotove)
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 12),
                    child: Center(child: CircularProgressIndicator()),
                  )
                else if (_slotoviZaDan.isEmpty)
                  Padding(
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    child: Text("Nema slobodnih termina za odabrani dan.",
                        style: TextStyle(color: Colors.grey.shade600)),
                  )
                else
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: _slotoviZaDan
                        .map((v) => ChoiceChip(
                              label: Text(v),
                              selected: _odabranoVrijeme == v,
                              onSelected: (_) =>
                                  setState(() => _odabranoVrijeme = v),
                            ))
                        .toList(),
                  ),
              ],
            ],
            const SizedBox(height: 20),
            const Text("Napomena (opciono)",
                style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            TextField(
              controller: _napomenaController,
              maxLines: 3,
              decoration: const InputDecoration(border: OutlineInputBorder()),
            ),
            const SizedBox(height: 28),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _saving ? null : _zakazi,
                child: _saving
                    ? const SizedBox(
                        height: 18,
                        width: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text("Zakaži termin"),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildLegenda() {
    return Row(
      children: [
        _legendaStavka(Colors.green.shade300, "Slobodno"),
        const SizedBox(width: 16),
        _legendaStavka(Colors.red.shade300, "Zauzeto"),
        const SizedBox(width: 16),
        _legendaStavka(Colors.grey.shade300, "Ne radi"),
      ],
    );
  }

  Widget _legendaStavka(Color boja, String tekst) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 12,
          height: 12,
          decoration: BoxDecoration(color: boja, shape: BoxShape.circle),
        ),
        const SizedBox(width: 4),
        Text(tekst, style: const TextStyle(fontSize: 12)),
      ],
    );
  }

  Widget _dan(DateTime day, {bool odabran = false, bool istaknutOkvir = false}) {
    final boja = _bojaDana(day);
    return Container(
      margin: const EdgeInsets.all(4),
      decoration: BoxDecoration(
        color: boja,
        shape: BoxShape.circle,
        border: odabran
            ? Border.all(color: Colors.black87, width: 2)
            : istaknutOkvir
                ? Border.all(color: Colors.blueGrey, width: 1.5)
                : null,
      ),
      alignment: Alignment.center,
      child: Text(
        '${day.day}',
        style: TextStyle(
          color: boja.computeLuminance() < 0.5 ? Colors.white : Colors.black87,
        ),
      ),
    );
  }

  Future _zakazi() async {
    if (_frizer == null) {
      alertBox(context, "Greška", "Izaberi frizera.");
      return;
    }
    if (_odabraniDatum == null || _odabranoVrijeme == null) {
      alertBox(context, "Greška", "Izaberi datum i vrijeme sa kalendara.");
      return;
    }

    final dijelovi = _odabranoVrijeme!.split(':');
    final datumVrijeme = DateTime(
      _odabraniDatum!.year,
      _odabraniDatum!.month,
      _odabraniDatum!.day,
      int.parse(dijelovi[0]),
      int.parse(dijelovi[1]),
    );

    setState(() => _saving = true);

    try {
      await _terminProvider.insert({
        "klijentId": _klijentId,
        "frizerId": _frizer!.id,
        "uslugaId": widget.usluga.id,
        "datumVrijeme": datumVrijeme.toIso8601String(),
        "napomena": _napomenaController.text,
      });

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Termin je uspješno zakazan!")),
      );
      Navigator.pop(context, "reload");
    } on Exception catch (e) {
      setState(() => _saving = false);
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }
}
