import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/frizer.dart';
import 'package:ecommerce_desktop/models/radno_vrijeme.dart';
import 'package:ecommerce_desktop/providers/radno_vrijeme_provider.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

/// Uredjivanje radnog vremena frizera po danu u sedmici - koristi ga color-coded
/// kalendar u mobilnoj app-i za racunanje slobodnih/zauzetih dana. Bez ovoga
/// (npr. za tek dodanog frizera) kalendar ce svaki dan prikazivati kao "Ne radi".
class RadnoVrijemeScreen extends StatefulWidget {
  final Frizer frizer;

  const RadnoVrijemeScreen({super.key, required this.frizer});

  @override
  State<RadnoVrijemeScreen> createState() => _RadnoVrijemeScreenState();
}

class _DanRedak {
  final int danUSedmici;
  int? postojeciId;
  bool neRadi;
  TextEditingController pocetak;
  TextEditingController kraj;

  _DanRedak({
    required this.danUSedmici,
    this.postojeciId,
    this.neRadi = true,
    String pocetak = "08:00",
    String kraj = "17:00",
  })  : pocetak = TextEditingController(text: pocetak),
        kraj = TextEditingController(text: kraj);
}

class _RadnoVrijemeScreenState extends State<RadnoVrijemeScreen> {
  late RadnoVrijemeProvider _provider;
  bool _isLoading = true;
  bool _saving = false;

  // Redoslijed prikaza Pon-Ned; vrijednosti odgovaraju System.DayOfWeek (0=Nedjelja).
  final List<int> _redoslijedDana = [1, 2, 3, 4, 5, 6, 0];
  final Map<int, String> _nazivDana = const {
    0: "Nedjelja",
    1: "Ponedjeljak",
    2: "Utorak",
    3: "Srijeda",
    4: "Četvrtak",
    5: "Petak",
    6: "Subota",
  };

  late Map<int, _DanRedak> _redci;

  @override
  void initState() {
    super.initState();
    _provider = context.read<RadnoVrijemeProvider>();
    _redci = {
      for (var d in _redoslijedDana) d: _DanRedak(danUSedmici: d),
    };
    _ucitaj();
  }

  Future _ucitaj() async {
    try {
      final result = await _provider
          .get(filter: {"frizerId": widget.frizer.id, "pageSize": 20});
      if (!mounted) return;
      setState(() {
        for (var rv in result.items ?? <RadnoVrijeme>[]) {
          final dan = rv.danUSedmici;
          if (dan == null || !_redci.containsKey(dan)) continue;
          _redci[dan] = _DanRedak(
            danUSedmici: dan,
            postojeciId: rv.id,
            neRadi: rv.neRadi ?? false,
            pocetak: rv.pocetakRada ?? "08:00",
            kraj: rv.krajRada ?? "17:00",
          );
        }
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      alertBox(context, "Greška", e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: "Radno vrijeme — ${widget.frizer.imePrezime ?? ''}",
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(32.0),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 700),
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : Column(
                    children: [
                      Card(
                        elevation: 6,
                        child: Padding(
                          padding: const EdgeInsets.all(16.0),
                          child: Column(
                            children: _redoslijedDana
                                .map((d) => _buildRedak(_redci[d]!))
                                .toList(),
                          ),
                        ),
                      ),
                      const SizedBox(height: 24),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: [
                          TextButton(
                            onPressed: () => Navigator.of(context).pop(),
                            child: const Text("Nazad"),
                          ),
                          const SizedBox(width: 16),
                          ElevatedButton(
                            onPressed: _saving ? null : _sacuvaj,
                            child: _saving
                                ? const SizedBox(
                                    height: 18,
                                    width: 18,
                                    child: CircularProgressIndicator(
                                        strokeWidth: 2),
                                  )
                                : const Text("Sačuvaj"),
                          ),
                        ],
                      ),
                    ],
                  ),
          ),
        ),
      ),
    );
  }

  Widget _buildRedak(_DanRedak redak) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6.0),
      child: Row(
        children: [
          SizedBox(
            width: 130,
            child: Text(_nazivDana[redak.danUSedmici] ?? ''),
          ),
          Expanded(
            child: Row(
              children: [
                Checkbox(
                  value: redak.neRadi,
                  onChanged: (v) => setState(() => redak.neRadi = v ?? false),
                ),
                const Text("Ne radi"),
                const SizedBox(width: 24),
                Expanded(
                  child: TextField(
                    controller: redak.pocetak,
                    enabled: !redak.neRadi,
                    decoration: const InputDecoration(
                      label: Text("Početak (HH:mm)"),
                      isDense: true,
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: TextField(
                    controller: redak.kraj,
                    enabled: !redak.neRadi,
                    decoration: const InputDecoration(
                      label: Text("Kraj (HH:mm)"),
                      isDense: true,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  bool _validanFormat(String v) {
    return RegExp(r'^([01]\d|2[0-3]):([0-5]\d)$').hasMatch(v);
  }

  Future _sacuvaj() async {
    for (var d in _redoslijedDana) {
      final redak = _redci[d]!;
      if (!redak.neRadi) {
        if (!_validanFormat(redak.pocetak.text) ||
            !_validanFormat(redak.kraj.text)) {
          alertBox(context, "Greška",
              "Vrijeme za ${_nazivDana[d]} mora biti u formatu HH:mm (npr. 08:00).");
          return;
        }
      }
    }

    setState(() => _saving = true);
    try {
      for (var d in _redoslijedDana) {
        final redak = _redci[d]!;
        final data = {
          "frizerId": widget.frizer.id,
          "danUSedmici": redak.danUSedmici,
          "pocetakRada": redak.neRadi ? "00:00" : redak.pocetak.text,
          "krajRada": redak.neRadi ? "00:00" : redak.kraj.text,
          "neRadi": redak.neRadi,
        };

        if (redak.postojeciId != null) {
          await _provider.update(redak.postojeciId!, data);
        } else {
          await _provider.insert(data);
        }
      }

      if (!mounted) return;
      Navigator.of(context).pop("reload");
    } on Exception catch (e) {
      if (!mounted) return;
      setState(() => _saving = false);
      alertBox(context, "Greška", e.toString());
    }
  }
}
