import 'package:json_annotation/json_annotation.dart';

part 'placanje.g.dart';

@JsonSerializable()
class Placanje {
  final int? id;
  final int? terminId;
  final double? iznos;
  final String? status;
  final String? paypalOrderId;
  final String? paypalTransactionId;
  final DateTime? createdAt;
  final DateTime? datumPlacanja;
  final DateTime? datumPovrata;

  Placanje({
    this.id,
    this.terminId,
    this.iznos,
    this.status,
    this.paypalOrderId,
    this.paypalTransactionId,
    this.createdAt,
    this.datumPlacanja,
    this.datumPovrata,
  });

  factory Placanje.fromJson(Map<String, dynamic> json) =>
      _$PlacanjeFromJson(json);

  Map<String, dynamic> toJson() => _$PlacanjeToJson(this);
}

@JsonSerializable()
class PlacanjeKreirajResponse {
  final int? placanjeId;
  final String? paypalOrderId;
  final String? approvalUrl;

  PlacanjeKreirajResponse({
    this.placanjeId,
    this.paypalOrderId,
    this.approvalUrl,
  });

  factory PlacanjeKreirajResponse.fromJson(Map<String, dynamic> json) =>
      _$PlacanjeKreirajResponseFromJson(json);

  Map<String, dynamic> toJson() => _$PlacanjeKreirajResponseToJson(this);
}
