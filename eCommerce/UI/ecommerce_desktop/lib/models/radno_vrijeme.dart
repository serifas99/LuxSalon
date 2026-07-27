import 'package:json_annotation/json_annotation.dart';

part 'radno_vrijeme.g.dart';

@JsonSerializable()
class RadnoVrijeme {
  final int? id;
  final int? frizerId;
  final String? frizerImePrezime;
  final int? danUSedmici;
  final String? danUSedmiceNaziv;
  final String? pocetakRada;
  final String? krajRada;
  final bool? neRadi;

  RadnoVrijeme({
    this.id,
    this.frizerId,
    this.frizerImePrezime,
    this.danUSedmici,
    this.danUSedmiceNaziv,
    this.pocetakRada,
    this.krajRada,
    this.neRadi,
  });

  factory RadnoVrijeme.fromJson(Map<String, dynamic> json) =>
      _$RadnoVrijemeFromJson(json);

  Map<String, dynamic> toJson() => _$RadnoVrijemeToJson(this);
}
